using Application.Contracts.Common;
using Application.Contracts.Posts.Request;
using Application.Services.Category;
using Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Authorization;
using WebApi.Extensions;

namespace WebApi.Controllers.AdminApi
{
    [Route("api/admin/category")]
    [ApiController]
    [Authorize]
    public class AdminCategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public AdminCategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        //Read
        [HttpGet("{categoryId}")]
        [HasPermission(Permissions.PostCategories.View)]
        public async Task<ActionResult> GetCategoryById(Guid categoryId)
        {
            var userId = User.GetUserId();
            var result = await _categoryService.GetCategoryByIdAsync(categoryId, userId);
            return Ok(result);
        }

        [HttpGet]
        [HasPermission(Permissions.PostCategories.View)]
        public async Task<ActionResult> GetAllCategories()
        {
            var userId = User.GetUserId();

            var result = await _categoryService.GetAllCategoriesAsync(userId);
            return Ok(result);
        }

        [HttpGet("paging")]
        [HasPermission(Permissions.PostCategories.View)]
        public async Task<ActionResult> GetCategoriesPaging([FromQuery] PagingRequest request)
        {
            var userId = User.GetUserId();
            var result = await _categoryService.GetCategoriesPagingAsync(request, userId);
            return Ok(result);
        }

        //Write

        [HttpPost]
        [HasPermission(Permissions.PostCategories.Create)]
        public async Task<ActionResult> CreateCategory(
            [FromBody] CreateUpdatePostCategoryRequest category
        )
        {
            var userId = User.GetUserId();
            await _categoryService.CreateCategoryAsync(category, userId);
            return Ok(WriteResponse.Success());
        }

        [HttpPut("{categoryId}")]
        [HasPermission(Permissions.PostCategories.Edit)]
        public async Task<ActionResult> UpdateCategory(
            Guid categoryId,
            [FromBody] CreateUpdatePostCategoryRequest category
        )
        {
            var userId = User.GetUserId();
            await _categoryService.UpdateCategoryAsync(categoryId, category, userId);
            return Ok(WriteResponse.Success());
        }

        [HttpDelete("{categoryId}")]
        [HasPermission(Permissions.PostCategories.Delete)]
        public async Task<ActionResult> DeleteCategory(Guid categoryId)
        {
            var userId = User.GetUserId();
            await _categoryService.DeleteCategoryAsync(categoryId, userId);
            return Ok(WriteResponse.Success());
        }
    }
}
