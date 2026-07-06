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

        public async Task<ReadResponse<PageResult<PostInListResponse>>> GetAllPostsAsync(
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

        public async Task<ReadResponse<PageResult<PostInListResponse>>> GetPostsByCategoryAsync(
            string categorySlug,
            PostPagingRequest request
        )
        {
            var posts = await _unitOfWork.Posts.GetPostsByCategoryAsync(categorySlug, request);
            return ReadResponse<PageResult<PostInListResponse>>.Success(posts);
        }

        public async Task<ReadResponse<PageResult<PostInListResponse>>> GetPostsByTagAsync(
            string tagSlug,
            PostPagingRequest request
        )
        {
            var posts = await _unitOfWork.Posts.GetPostsByTagAsync(tagSlug, request);
            return ReadResponse<PageResult<PostInListResponse>>.Success(posts);
        }

        public async Task<ReadResponse<PageResult<PostInListResponse>>> GetPublishedPostsAsync(
            PostPagingRequest request
        )
        {
            var posts = await _unitOfWork.Posts.GetPublishedPostsAsync(request);
            return ReadResponse<PageResult<PostInListResponse>>.Success(posts);
        }

        public async Task<WriteResponse> CreatePostAsync(
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

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return WriteResponse.Failure(ErrorMessages.User.UserNotFound);

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
            if (request.Tags != null && request.Tags.Length > 0)
            {
                //looping through the tags and set the tag info
                foreach (var tagName in request.Tags)
                {
                    var tagSlug = TextHelper.GenerateSlug(tagName);
                    var tag = await _unitOfWork
                        .Tags.Find(t => t.Slug == tagSlug)
                        .FirstOrDefaultAsync();
                    Guid tagId;
                    if (tag == null)
                    {
                        tagId = Guid.NewGuid();
                        _unitOfWork.Tags.Add(
                            new Tag()
                            {
                                Id = tagId,
                                Name = tagName,
                                Slug = tagSlug,
                            }
                        );
                    }
                    else
                    {
                        tagId = tag.Id;
                    }
                    await _unitOfWork.PostTags.AddTagToPostAsync(post.Id, tagId);
                }
            }
            var result = await _unitOfWork.CompleteAsync();
            return result > 0
                ? WriteResponse.Success()
                : WriteResponse.Failure(ErrorMessages.Post.CreatePostFailed);
        }
    }
}
