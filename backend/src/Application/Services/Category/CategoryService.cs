using Application.Constants;
using Application.Contracts.Common;
using Application.Contracts.Posts.Response;
using Application.Services.Permission;
using Application.UnitOfWork;
using AutoMapper;
using Domain;
using Domain.Cores.Content;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using static Application.Exceptions.CustomException;
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

        public async Task<List<PostCategoryResponse>> GetAllActiveCategoriesAsync()
        {
            var categories = _unitOfWork.Categories.Find(c => c.IsActive);

            return await _mapper.ProjectTo<PostCategoryResponse>(categories).ToListAsync();
        }

        public async Task<PostCategoryResponse> GetActiveCategoryByIdAsync(Guid categoryId)
        {
            var category = await _unitOfWork
                .Categories.Find(c => c.IsActive && c.Id == categoryId)
                .FirstOrDefaultAsync();
            if (category == null)
                throw new NotFoundException(ErrorMessages.Category.CategoryNotFound);
            return _mapper.Map<PostCategory, PostCategoryResponse>(category);
        }

        public async Task<PageResult<PostCategoryResponse>> GetActiveCategoriesPagingAsync(
            PagingRequest request
        )
        {
            var result = await _unitOfWork.Categories.GetActiveCategoriesPagingAsync(request);
            return result;
        }
    }
}
