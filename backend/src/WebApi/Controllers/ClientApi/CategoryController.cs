using Application.Contracts.Common;
using Application.Services.Category;
using Microsoft.AspNetCore.Mvc;
using WebApi.Extensions;

namespace WebApi.Controllers.ClientApi
{
    [Route("api/category")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        //Read
        [HttpGet("{categoryId}")]
        public async Task<ActionResult> GetActiveCategoryById(Guid categoryId)
        {
            var result = await _categoryService.GetActiveCategoryByIdAsync(categoryId);
            return Ok(result);
        }

        [HttpGet]
        public async Task<ActionResult> GetActiveAllCategories()
        {
            var result = await _categoryService.GetAllActiveCategoriesAsync();
            return Ok(result);
        }

        [HttpGet("paging")]
        public async Task<ActionResult> GetActiveCategoriesPaging([FromQuery] PagingRequest request)
        {
            var result = await _categoryService.GetActiveCategoriesPagingAsync(request);
            return Ok(result);
        }
    }
}
