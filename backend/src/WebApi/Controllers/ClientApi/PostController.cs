using Application.Contracts.Common;
using Application.Services.Post;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers.ClientApi
{
    [Route("api/posts")]
    [ApiController]
    public class PostController : ControllerBase
    {
        private readonly IClientPostService _postService;

        public PostController(IClientPostService postService)
        {
            _postService = postService;
        }

        [HttpGet("paging")]
        public async Task<ActionResult> GetPublishedPosts([FromQuery] PagingRequest request)
        {
            var result = await _postService.GetAllPostsAsync(request);
            return Ok(result);
        }

        [HttpGet("category/{categorySlug}")]
        public async Task<ActionResult> GetPostsByCategory(
            [FromRoute] string categorySlug,
            [FromQuery] PagingRequest request
        )
        {
            var result = await _postService.GetPostsByCategoryAsync(categorySlug, request);
            return Ok(result);
        }

        [HttpGet("tag/{tagSlug}")]
        public async Task<ActionResult> GetPostsByTag(
            [FromRoute] string tagSlug,
            [FromQuery] PagingRequest request
        )
        {
            var result = await _postService.GetPostsByTagAsync(tagSlug, request);
            return Ok(result);
        }

        [HttpGet("{postId}")]
        public async Task<ActionResult> GetPostById([FromRoute] Guid postId)
        {
            var result = await _postService.GetPostByIdAsync(postId);
            return Ok(result);
        }
    }
}
