using Application.Constants;
using Application.Contracts.Posts.Response;
using Application.Services.Permission;
using Application.UnitOfWork;
using AutoMapper;
using static Application.Exceptions.CustomException;
using Microsoft.AspNetCore.Identity;
using AppUser = Domain.Cores.Identity.User;

namespace Application.Services.Category
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<AppUser> _userManager;
        private readonly IMapper _mapper;
        private readonly IPermissionService _permissionService;

        public CategoryService(
            IUnitOfWork unitOfWork,
            UserManager<AppUser> userManager,
            IMapper mapper,
            IPermissionService permissionService
        )
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _mapper = mapper;
            _permissionService = permissionService;
        }

        public async Task<PostCategoryResponse> GetCategoryByIdAsync(
            Guid categoryId,
            Guid currentUserId
        )
        {
            var user = await _userManager.FindByIdAsync(currentUserId.ToString())
                ?? throw new NotFoundException(ErrorMessages.User.UserNotFound);

            var category = await _unitOfWork.Categories.GetByIdAsync(categoryId)
                ?? throw new NotFoundException(ErrorMessages.Category.CategoryNotFound);

            var hasViewActiveCategoryPermission =
                _permissionService.HasViewActiveCategoryPermission(
                    currentUserId,
                    category.IsActive
                );
            if (!hasViewActiveCategoryPermission)
                throw new ForbiddenException(ErrorMessages.Category.InsufficientPermissions);

            return _mapper.Map<PostCategoryResponse>(category);
        }
    }
}
