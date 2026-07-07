using Application.Contracts.Common;
using Application.Contracts.Posts.Request;
using Application.Contracts.Posts.Response;
using Application.Services.Post;
using Domain;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers.ClientApi
{
    [Route("api/posts")]
    [ApiController]
    public class PostController : ApiControllerBase
    {
        private readonly IPostService _postService;

        public PostController(IPostService postService)
        {
            _postService = postService;
        }

        [HttpGet]
        public async Task<
            ActionResult<ReadResponse<PageResult<PostInListResponse>>>
        > GetPublishedPosts([FromQuery] PostPagingRequest request)
        {
            var result = await _postService.ClientGetAllPostsAsync(request);
            return ToActionResult(result);
        }

        [HttpGet("category/{categorySlug}")]
        public async Task<
            ActionResult<ReadResponse<PageResult<PostInListResponse>>>
        > GetPostsByCategory([FromRoute] string categorySlug, [FromQuery] PostPagingRequest request)
        {
            var result = await _postService.ClientGetPostsByCategoryAsync(categorySlug, request);
            return ToActionResult(result);
        }

        [HttpGet("tag/{tagSlug}")]
        public async Task<ActionResult<ReadResponse<PageResult<PostInListResponse>>>> GetPostsByTag(
            [FromRoute] string tagSlug,
            [FromQuery] PostPagingRequest request
        )
        {
            var result = await _postService.ClientGetPostsByTagAsync(tagSlug, request);
            return ToActionResult(result);
        }

        [HttpGet("{postId}")]
        public async Task<ActionResult<ReadResponse<PostResponse>>> GetPostById(
            [FromRoute] Guid postId
        )
        {
            var result = await _postService.ClientGetPostByIdAsync(postId);
            return ToActionResult(result);
        }
    }
}
