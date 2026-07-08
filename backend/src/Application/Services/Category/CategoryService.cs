using Application.Constants;
using Application.Contracts.Common;
using Application.Contracts.Posts.Request;
using Application.Contracts.Posts.Response;
using Application.Services.Permission;
using Application.UnitOfWork;
using AutoMapper;
using Domain;
using Domain.Cores.Content;
using Microsoft.EntityFrameworkCore;
using static Application.Exceptions.CustomException;

namespace Application.Services.Category
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IPermissionService _permissionService;

        public CategoryService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IPermissionService permissionService
        )
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _permissionService = permissionService;
        }

        // Admin
        public async Task<PostCategoryResponse> GetCategoryByIdAsync(Guid categoryId, Guid userId)
        {
            var hasViewPermission = _permissionService.HasViewAllCategoryPermission(userId);
            if (!hasViewPermission)
                throw new ForbiddenException(ErrorMessages.Category.InsufficientPermissions);
            var category = await _unitOfWork
                .Categories.Find(c => c.Id == categoryId)
                .FirstOrDefaultAsync();
            if (category == null)
                throw new NotFoundException(ErrorMessages.Category.CategoryNotFound);
            return _mapper.Map<PostCategory, PostCategoryResponse>(category);
        }

        public async Task<List<PostCategoryResponse>> GetAllCategoriesAsync(Guid userId)
        {
            var hasViewPermission = _permissionService.HasViewAllCategoryPermission(userId);
            if (!hasViewPermission)
                throw new ForbiddenException(ErrorMessages.Category.InsufficientPermissions);
            var categories = _unitOfWork.Categories.Find(c => true);
            return await _mapper.ProjectTo<PostCategoryResponse>(categories).ToListAsync();
        }

        public async Task<PageResult<PostCategoryResponse>> GetCategoriesPagingAsync(
            PagingRequest request,
            Guid userId
        )
        {
            var hasViewPermission = _permissionService.HasViewAllCategoryPermission(userId);
            if (!hasViewPermission)
                throw new ForbiddenException(ErrorMessages.Category.InsufficientPermissions);
            return await _unitOfWork.Categories.GetCategoriesPagingAsync(request);
        }

        public async Task<PostCategoryResponse> CreateCategoryAsync(
            CreateUpdatePostCategoryRequest request,
            Guid userId
        )
        {
            var hasCreatePermission = _permissionService.HasCreateCategoryPermission(userId);
            if (!hasCreatePermission)
                throw new ForbiddenException(ErrorMessages.Category.InsufficientPermissions);
            var slugExists = await _unitOfWork
                .Categories.Find(c => c.Slug == request.Slug)
                .AnyAsync();
            if (slugExists)
                throw new BadRequestException(ErrorMessages.Category.SlugAlreadyExists);

            var category = _mapper.Map<CreateUpdatePostCategoryRequest, PostCategory>(request);
            _unitOfWork.Categories.Add(category);

            var result = await _unitOfWork.CompleteAsync();
            if (result <= 0)
                throw new BadRequestException(ErrorMessages.Category.CreateFailed);

            return _mapper.Map<PostCategory, PostCategoryResponse>(category);
        }

        public async Task UpdateCategoryAsync(
            Guid categoryId,
            CreateUpdatePostCategoryRequest request,
            Guid userId
        )
        {
            var hasEditPermission = _permissionService.HasEditCategoryPermission(userId);
            if (!hasEditPermission)
                throw new ForbiddenException(ErrorMessages.Category.InsufficientPermissions);

            var category =
                await _unitOfWork.Categories.Find(c => c.Id == categoryId).FirstOrDefaultAsync()
                ?? throw new NotFoundException(ErrorMessages.Category.CategoryNotFound);

            var slugExists = await _unitOfWork
                .Categories.Find(c => c.Slug == request.Slug && c.Id != categoryId)
                .AnyAsync();
            if (slugExists)
                throw new BadRequestException(ErrorMessages.Category.SlugAlreadyExists);

            _mapper.Map(request, category);

            var result = await _unitOfWork.CompleteAsync();
            if (result <= 0)
                throw new BadRequestException(ErrorMessages.Category.UpdateFailed);
        }

        public async Task DeleteCategoryAsync(Guid categoryId, Guid userId)
        {
            var hasDeletePermission = _permissionService.HasDeleteCategoryPermission(userId);
            if (!hasDeletePermission)
                throw new ForbiddenException(ErrorMessages.Category.InsufficientPermissions);

            var category =
                await _unitOfWork.Categories.Find(c => c.Id == categoryId).FirstOrDefaultAsync()
                ?? throw new NotFoundException(ErrorMessages.Category.CategoryNotFound);

            _unitOfWork.Categories.Remove(category);

            var result = await _unitOfWork.CompleteAsync();
            if (result <= 0)
                throw new BadRequestException(ErrorMessages.Category.DeleteFailed);
        }

        // Client
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
