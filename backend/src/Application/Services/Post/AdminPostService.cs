using Application.Constants;
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
using static Application.Exceptions.CustomException;
using AppPost = Domain.Cores.Content.Post;
using AppUser = Domain.Cores.Identity.User;

namespace Application.Services.Post
{
    public class AdminPostService : IAdminPostService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IPermissionService _permissionService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public AdminPostService(
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

        //Read
        public async Task<PageResult<PostInListResponse>> GetAllPostsAsync(
            GetAllPostsRequest request,
            Guid currentUserId
        )
        {
            var user =
                await _userManager.FindByIdAsync(currentUserId.ToString())
                ?? throw new NotFoundException(ErrorMessages.User.UserNotFound);

            var hasApprovePostPermission = _permissionService.HasApprovedPostPermission(user.Id);

            return await _unitOfWork.Posts.GetAllPostsAsync(
                request,
                currentUserId,
                hasApprovePostPermission
            );
        }

        public async Task<AppPost> GetPostByIdAsync(Guid postId, Guid currentUserId)
        {
            var post =
                await _unitOfWork.Posts.Find(p => p.Id == postId).FirstOrDefaultAsync()
                ?? throw new NotFoundException(ErrorMessages.Post.PostNotFound);

            var user =
                await _userManager.FindByIdAsync(currentUserId.ToString())
                ?? throw new NotFoundException(ErrorMessages.User.UserNotFound);

            if (user.Id == post.AuthorUserId)
                return post;

            if (post.Status == PostStatus.Draft)
                throw new NotFoundException(ErrorMessages.Post.PostNotFound);

            if (!_permissionService.HasApprovedPostPermission(user.Id))
                throw new ForbiddenException(ErrorMessages.Post.InsufficientPostPermission);

            return post;
        }

        public async Task<List<PostActivityLog>> GetActivityLogsAsync(Guid postId, Guid userId)
        {
            var post =
                await _unitOfWork.Posts.Find(p => p.Id == postId).FirstOrDefaultAsync()
                ?? throw new NotFoundException(ErrorMessages.Post.PostNotFound);

            var user =
                await _userManager.FindByIdAsync(userId.ToString())
                ?? throw new NotFoundException(ErrorMessages.User.UserNotFound);

            return await _unitOfWork.PostActivityLogs.GetActivityLogsAsync(post, user);
        }

        //Write
        public async Task CreatePostAsync(CreateUpdatePostRequest request, Guid userId)
        {
            var existingPost = await _unitOfWork
                .Posts.Find(p => p.Slug == request.Slug)
                .FirstOrDefaultAsync();
            if (existingPost != null)
                throw new BadRequestException(ErrorMessages.Post.SlugAlreadyExists);

            var user =
                await _userManager.FindByIdAsync(userId.ToString())
                ?? throw new NotFoundException(ErrorMessages.User.UserNotFound);

            var post = _mapper.Map<CreateUpdatePostRequest, AppPost>(request);
            post.AuthorUserId = userId;
            post.AuthorUserName = user.UserName;
            post.AuthorName = user.GetFullName();

            var category =
                await _unitOfWork
                    .Categories.Find(c => c.Id == request.CategoryId)
                    .FirstOrDefaultAsync()
                ?? throw new NotFoundException(ErrorMessages.Category.CategoryNotFound);

            post.CategoryId = category.Id;
            post.CategoryName = category.Name;
            post.CategorySlug = category.Slug;
            _unitOfWork.Posts.Add(post);

            await ProcessTagsAsync(post.Id, request.Tags);

            var result = await _unitOfWork.CompleteAsync();
            if (result <= 0)
                throw new BadRequestException(ErrorMessages.Post.CreatePostFailed);
        }

        public async Task UpdatePostAsync(CreateUpdatePostRequest request, Guid postId, Guid userId)
        {
            var post =
                await _unitOfWork.Posts.Find(p => p.Id == postId).FirstOrDefaultAsync()
                ?? throw new NotFoundException(ErrorMessages.Post.PostNotFound);

            var slugExists = await _unitOfWork
                .Posts.Find(p => p.Slug == request.Slug && p.Id != postId)
                .AnyAsync();
            if (slugExists)
                throw new BadRequestException(ErrorMessages.Post.SlugAlreadyExists);

            _mapper.Map(request, post);

            var category =
                await _unitOfWork
                    .Categories.Find(c => c.Id == request.CategoryId)
                    .FirstOrDefaultAsync()
                ?? throw new NotFoundException(ErrorMessages.Category.CategoryNotFound);
            post.CategoryName = category.Name;
            post.CategorySlug = category.Slug;

            _unitOfWork.PostTags.ClearAllTagsFromPost(post.Id);
            await ProcessTagsAsync(post.Id, request.Tags);

            var result = await _unitOfWork.CompleteAsync();
            if (result <= 0)
                throw new BadRequestException(ErrorMessages.Post.UpdatePostFailed);
        }

        public async Task DeletePostAsync(Guid[] ids, Guid currentUserId)
        {
            var user =
                await _userManager.FindByIdAsync(currentUserId.ToString())
                ?? throw new NotFoundException(ErrorMessages.User.UserNotFound);

            var hasDeletePostPermission = _permissionService.HasDeletePostPermission(user.Id);
            var posts = await _unitOfWork.Posts.Find(p => ids.Contains(p.Id)).ToListAsync();

            if (posts.Count == 0 || posts.Count != ids.Length)
                throw new NotFoundException(ErrorMessages.Post.PostNotFound);

            if (!hasDeletePostPermission && posts.Any(p => p.AuthorUserId != currentUserId))
                throw new ForbiddenException(ErrorMessages.Post.InsufficientPostPermission);

            _unitOfWork.Posts.RemoveRange(posts);
            foreach (var post in posts)
            {
                _unitOfWork.PostTags.ClearAllTagsFromPost(post.Id);
                _unitOfWork.PostInSeries.ClearPostFromAllSeries(post.Id);
            }

            var result = await _unitOfWork.CompleteAsync();
            if (result <= 0)
                throw new BadRequestException(ErrorMessages.Post.DeleteFailed);
        }

        public async Task ApprovePostAsync(Guid postId, Guid currentUserId, string? note)
        {
            var post =
                await _unitOfWork.Posts.Find(p => p.Id == postId).FirstOrDefaultAsync()
                ?? throw new NotFoundException(ErrorMessages.Post.PostNotFound);

            var user =
                await _userManager.FindByIdAsync(currentUserId.ToString())
                ?? throw new NotFoundException(ErrorMessages.User.UserNotFound);

            var approved = await _unitOfWork.Posts.Approve(post, user, note);
            if (!approved)
                throw new BadRequestException(ErrorMessages.Post.ApproveFailed);

            var result = await _unitOfWork.CompleteAsync();
            if (result <= 0)
                throw new BadRequestException(ErrorMessages.Post.ApproveFailed);
        }

        public async Task RejectPostAsync(Guid postId, Guid currentUserId, string? note)
        {
            var post =
                await _unitOfWork.Posts.Find(p => p.Id == postId).FirstOrDefaultAsync()
                ?? throw new NotFoundException(ErrorMessages.Post.PostNotFound);

            var user =
                await _userManager.FindByIdAsync(currentUserId.ToString())
                ?? throw new NotFoundException(ErrorMessages.User.UserNotFound);

            var rejected = await _unitOfWork.Posts.Reject(post, user, note);
            if (!rejected)
                throw new BadRequestException(ErrorMessages.Post.RejectFailed);

            var result = await _unitOfWork.CompleteAsync();
            if (result <= 0)
                throw new BadRequestException(ErrorMessages.Post.RejectFailed);
        }

        public async Task SubmitPostForApprovalAsync(Guid postId, Guid userId, string? note)
        {
            var post =
                await _unitOfWork.Posts.Find(p => p.Id == postId).FirstOrDefaultAsync()
                ?? throw new NotFoundException(ErrorMessages.Post.PostNotFound);

            var user =
                await _userManager.FindByIdAsync(userId.ToString())
                ?? throw new NotFoundException(ErrorMessages.User.UserNotFound);

            if (post.AuthorUserId != userId)
                throw new ForbiddenException(ErrorMessages.Post.InsufficientPostPermission);

            var submit = await _unitOfWork.Posts.SubmitForApproval(post, user, note);
            if (!submit)
                throw new BadRequestException(ErrorMessages.Post.SubmitForApprovalFailed);

            var result = await _unitOfWork.CompleteAsync();
            if (result <= 0)
                throw new BadRequestException(ErrorMessages.Post.SubmitForApprovalFailed);
        }

        public async Task<string> GetRejectReasonAsync(Guid postId, Guid userId)
        {
            var post =
                await _unitOfWork.Posts.Find(p => p.Id == postId).FirstOrDefaultAsync()
                ?? throw new NotFoundException(ErrorMessages.Post.PostNotFound);

            var user =
                await _userManager.FindByIdAsync(userId.ToString())
                ?? throw new NotFoundException(ErrorMessages.User.UserNotFound);

            if (post.Status != PostStatus.Rejected)
                throw new BadRequestException(ErrorMessages.Post.PostNotRejected);

            if (
                post.AuthorUserId != userId
                && _permissionService.HasApprovedPostPermission(user.Id)
            )
                throw new ForbiddenException(ErrorMessages.Post.InsufficientPostPermission);

            var result = await _unitOfWork.PostActivityLogs.GetRejectReasonAsync(post, user);
            if (string.IsNullOrEmpty(result))
                throw new BadRequestException(ErrorMessages.Post.FailToGetRejectReason);

            return result;
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
