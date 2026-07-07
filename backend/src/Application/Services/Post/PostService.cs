using Application.Constants;
using Application.Contracts.Common;
using Application.Contracts.Posts.Request;
using Application.Contracts.Posts.Response;
using Application.Helper;
using Application.Services.Permission;
using Application.UnitOfWork;
using AutoMapper;
using Domain;
using Domain.Cores.Content;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AppPost = Domain.Cores.Content.Post;
using AppUser = Domain.Cores.Identity.User;

namespace Application.Services.Post
{
    public class PostService : IPostService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IPermissionService _permissionService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public PostService(
            UserManager<AppUser> userManager,
            IPermissionService permissionService,
            IUnitOfWork unitOfWork,
            IMapper mapper
        )
        {
            _userManager = userManager;
            _permissionService = permissionService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        // Admin
        public async Task<ReadResponse<PageResult<PostInListResponse>>> AdminGetAllPostsAsync(
            GetAllPostsRequest request,
            Guid currentUserId
        )
        {
            //Find user by id
            var user = await _userManager.FindByIdAsync(currentUserId.ToString());
            if (user == null)
                return ReadResponse<PageResult<PostInListResponse>>.Failure(
                    ErrorMessages.User.UserNotFound
                );

            //Check user permissions to approve post
            var hasApprovePostPermission = _permissionService.HasApprovedPostPermission(user.Id);

            //Calling query to get all posts with filters and pagination
            var posts = await _unitOfWork.Posts.GetAllPostsAsync(
                request,
                currentUserId,
                hasApprovePostPermission
            );
            return ReadResponse<PageResult<PostInListResponse>>.Success(posts);
        }

        public async Task<ReadResponse<AppPost>> AdminGetPostByIdAsync(
            Guid postId,
            Guid currentUserId
        )
        {
            var post = await _unitOfWork.Posts.Find(p => p.Id == postId).FirstOrDefaultAsync();
            if (post == null)
                return ReadResponse<AppPost>.Failure(ErrorMessages.Post.PostNotFound);

            //Check current user is the author of the post or not
            var user = await _userManager.FindByIdAsync(currentUserId.ToString());
            if (user == null)
                return ReadResponse<AppPost>.Failure(ErrorMessages.User.UserNotFound);

            //Author can view their own post (including draft)
            if (user.Id == post.AuthorUserId)
                return ReadResponse<AppPost>.Success(post);

            //Only authors can view their own draft post
            if (post.Status == PostStatus.Draft)
                return ReadResponse<AppPost>.Failure(ErrorMessages.Post.PostNotFound);

            var hasApprovePostPermission = _permissionService.HasApprovedPostPermission(user.Id);
            // Editors, Admins can view all non-draft posts
            if (!hasApprovePostPermission)
                return ReadResponse<AppPost>.Failure(ErrorMessages.Post.InsufficientPostPermission);
            return ReadResponse<AppPost>.Success(post);
        }

        public async Task<WriteResponse> AdminCreatePostAsync(
            CreateUpdatePostRequest request,
            Guid userId
        )
        {
            //Check if post-slug is already exists
            var existingPost = await _unitOfWork
                .Posts.Find(p => p.Slug == request.Slug)
                .FirstOrDefaultAsync();
            if (existingPost != null)
                return WriteResponse.Failure(ErrorMessages.Post.SlugAlreadyExists);
            //Set author info

            var (user, userError) = await GetUserAndValidateAsync(userId);
            if (userError != null)
                return userError;

            var post = _mapper.Map<CreateUpdatePostRequest, AppPost>(request);
            post.AuthorUserId = userId;
            post.AuthorUserName = user.UserName;
            post.AuthorName = user.GetFullName();

            //Set Category info
            var category = await _unitOfWork
                .Categories.Find(c => c.Id == request.CategoryId)
                .FirstOrDefaultAsync();
            if (category == null)
                return WriteResponse.Failure(ErrorMessages.Category.CategoryNotFound);

            post.CategoryId = category.Id;
            post.CategoryName = category.Name;
            post.CategorySlug = category.Slug;
            _unitOfWork.Posts.Add(post);

            //Set tags info
            await ProcessTagsAsync(post.Id, request.Tags);

            var result = await _unitOfWork.CompleteAsync();
            return result > 0
                ? WriteResponse.Success()
                : WriteResponse.Failure(ErrorMessages.Post.CreatePostFailed);
        }

        public async Task<WriteResponse> AdminUpdatePostAsync(
            CreateUpdatePostRequest request,
            Guid postId,
            Guid userId
        )
        {
            var (post, postError) = await GetPostAndValidateAsync(postId);
            if (postError != null)
                return postError;

            //check slug uniqueness
            var slugExists = await _unitOfWork
                .Posts.Find(p => p.Slug == request.Slug && p.Id != postId)
                .AnyAsync();
            if (slugExists)
                return WriteResponse.Failure(ErrorMessages.Post.SlugAlreadyExists);

            //mapping request onto tracked entity
            _mapper.Map(request, post);

            //update category denormalized fields
            var category = await _unitOfWork
                .Categories.Find(c => c.Id == request.CategoryId)
                .FirstOrDefaultAsync();
            if (category == null)
                return WriteResponse.Failure(ErrorMessages.Category.CategoryNotFound);
            post.CategoryName = category.Name;
            post.CategorySlug = category.Slug;

            //clear old tags then add new
            _unitOfWork.PostTags.ClearAllTagsFromPost(post.Id);
            await ProcessTagsAsync(post.Id, request.Tags);

            var result = await _unitOfWork.CompleteAsync();
            return result > 0
                ? WriteResponse.Success()
                : WriteResponse.Failure(ErrorMessages.Post.UpdatePostFailed);
        }

        public async Task<WriteResponse> AdminDeletePostAsync(Guid[] ids, Guid currentUserId)
        {
            var (user, userError) = await GetUserAndValidateAsync(currentUserId);
            if (userError != null)
                return userError;
            var hasDeletePostPermission = _permissionService.HasDeletePostPermission(user.Id);
            var posts = await _unitOfWork.Posts.Find(p => ids.Contains(p.Id)).ToListAsync();
            if (posts.Count == 0 || posts.Count != ids.Length)
                return WriteResponse.Failure(ErrorMessages.Post.PostNotFound);

            if (!hasDeletePostPermission && posts.Any(p => p.AuthorUserId != currentUserId))
            {
                return WriteResponse.Failure(ErrorMessages.Post.InsufficientPostPermission);
            }

            _unitOfWork.Posts.RemoveRange(posts);
            foreach (var post in posts)
            {
                _unitOfWork.PostTags.ClearAllTagsFromPost(post.Id);
                _unitOfWork.PostInSeries.ClearPostFromAllSeries(post.Id);
            }
            var result = await _unitOfWork.CompleteAsync();
            return result > 0
                ? WriteResponse.Success()
                : WriteResponse.Failure(ErrorMessages.Post.DeleteFailed);
        }

        public async Task<WriteResponse> AdminApprovePostAsync(
            Guid postId,
            Guid currentUserId,
            string? note
        )
        {
            var (post, postError) = await GetPostAndValidateAsync(postId);
            if (postError != null)
                return postError;
            var (user, userError) = await GetUserAndValidateAsync(currentUserId);
            if (userError != null)
                return userError;
            var approved = await _unitOfWork.Posts.Approve(postId, currentUserId, note);
            if (!approved)
                return WriteResponse.Failure(ErrorMessages.Post.ApproveFailed);
            var result = await _unitOfWork.CompleteAsync();
            return result > 0
                ? WriteResponse.Success()
                : WriteResponse.Failure(ErrorMessages.Post.ApproveFailed);
        }

        public async Task<WriteResponse> AdminRejectPostAsync(
            Guid postId,
            Guid currentUserId,
            string? note
        )
        {
            var (post, postError) = await GetPostAndValidateAsync(postId);
            if (postError != null)
                return postError;
            var (user, userError) = await GetUserAndValidateAsync(currentUserId);
            if (userError != null)
                return userError;
            var rejected = await _unitOfWork.Posts.Reject(postId, currentUserId, note);
            if (!rejected)
                return WriteResponse.Failure(ErrorMessages.Post.RejectFailed);
            var result = await _unitOfWork.CompleteAsync();
            return result > 0
                ? WriteResponse.Success()
                : WriteResponse.Failure(ErrorMessages.Post.RejectFailed);
        }

        public async Task<WriteResponse> AdminSubmitPostForApprovalAsync(
            Guid postId,
            Guid userId,
            string? note
        )
        {
            var (post, postError) = await GetPostAndValidateAsync(postId);
            if (postError != null)
                return postError;
            var (user, userError) = await GetUserAndValidateAsync(userId);
            if (userError != null)
                return userError;
            if (post.AuthorUserId != userId)
                return WriteResponse.Failure(ErrorMessages.Post.InsufficientPostPermission);

            var submit = await _unitOfWork.Posts.SubmitForApproval(postId, userId, note);
            if (!submit)
                return WriteResponse.Failure(ErrorMessages.Post.SubmitForApprovalFailed);
            var result = await _unitOfWork.CompleteAsync();
            return result > 0
                ? WriteResponse.Success()
                : WriteResponse.Failure(ErrorMessages.Post.SubmitForApprovalFailed);
        }

        // Client
        public async Task<ReadResponse<PageResult<PostInListResponse>>> ClientGetAllPostsAsync(
            PostPagingRequest request
        )
        {
            var posts = await _unitOfWork.Posts.GetPublishedPostsAsync(request);
            return ReadResponse<PageResult<PostInListResponse>>.Success(posts);
        }

        public async Task<ReadResponse<PostResponse>> ClientGetPostByIdAsync(Guid postId)
        {
            var post = await _unitOfWork.Posts.Find(p => p.Id == postId).FirstOrDefaultAsync();
            if (post == null || post.Status != PostStatus.Published)
                return ReadResponse<PostResponse>.Failure(ErrorMessages.Post.PostNotFound);
            var postResponse = _mapper.Map<AppPost, PostResponse>(post);
            return ReadResponse<PostResponse>.Success(postResponse);
        }

        public async Task<
            ReadResponse<PageResult<PostInListResponse>>
        > ClientGetPostsByCategoryAsync(string categorySlug, PostPagingRequest request)
        {
            var posts = await _unitOfWork.Posts.GetPostsByCategoryAsync(categorySlug, request);
            return ReadResponse<PageResult<PostInListResponse>>.Success(posts);
        }

        public async Task<ReadResponse<PageResult<PostInListResponse>>> ClientGetPostsByTagAsync(
            string tagSlug,
            PostPagingRequest request
        )
        {
            var posts = await _unitOfWork.Posts.GetPostsByTagAsync(tagSlug, request);
            return ReadResponse<PageResult<PostInListResponse>>.Success(posts);
        }

        // Private helpers
        private async Task<(AppPost Post, WriteResponse? Error)> GetPostAndValidateAsync(
            Guid postId
        )
        {
            var post = await _unitOfWork.Posts.Find(p => p.Id == postId).FirstOrDefaultAsync();
            return post == null
                ? (null!, WriteResponse.Failure(ErrorMessages.Post.PostNotFound))
                : (post, null);
        }

        private async Task<(AppUser User, WriteResponse? Error)> GetUserAndValidateAsync(
            Guid userId
        )
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            return user == null
                ? (null!, WriteResponse.Failure(ErrorMessages.User.UserNotFound))
                : (user, null);
        }

        private async Task ProcessTagsAsync(Guid postId, string[] tags)
        {
            if (tags == null || tags.Length == 0)
                return;

            foreach (var tagName in tags)
            {
                var tagSlug = TextHelper.GenerateSlug(tagName);
                var tag = await _unitOfWork.Tags.Find(t => t.Slug == tagSlug).FirstOrDefaultAsync();

                var tagId = tag?.Id ?? Guid.NewGuid();

                if (tag == null)
                {
                    _unitOfWork.Tags.Add(
                        new Tag
                        {
                            Id = tagId,
                            Name = tagName,
                            Slug = tagSlug,
                        }
                    );
                }

                _unitOfWork.PostTags.AddTagToPost(postId, tagId);
            }
        }
    }
}
