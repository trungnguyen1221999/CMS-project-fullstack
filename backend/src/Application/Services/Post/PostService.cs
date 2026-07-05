using Application.Constants;
using Application.Contracts.Common;
using Application.Contracts.Posts.Request;
using Application.Contracts.Posts.Response;
using Application.Services.Permission;
using Application.UnitOfWork;
using Domain;
using Microsoft.AspNetCore.Identity;
using AppUser = Domain.Cores.Identity.User;

namespace Application.Services.Post
{
    public class PostService : IPostService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IPermissionService _permissionService;
        private readonly IUnitOfWork _unitOfWork;

        public PostService(
            UserManager<AppUser> userManager,
            IPermissionService permissionService,
            IUnitOfWork unitOfWork
        )
        {
            _userManager = userManager;
            _permissionService = permissionService;
            _unitOfWork = unitOfWork;
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
    }
}
