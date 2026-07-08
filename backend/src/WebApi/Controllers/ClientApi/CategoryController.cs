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
        public async Task<ActionResult> GetCategoryById(
            Guid categoryId
        )
        {
            var currentUserId = User.GetUserId();
            var result = await _categoryService.GetCategoryByIdAsync(categoryId, currentUserId);
            return Ok(result);
        }

        //Write
    }
}
