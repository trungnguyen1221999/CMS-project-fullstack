using Application.Contracts.Common;
using Application.Contracts.Posts.Request;
using Application.Contracts.Posts.Response;
using Application.Services.Post;
using Domain;
using Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Authorization;
using WebApi.Extensions;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PostController : ApiControllerBase
    {
        private readonly IPostService _postService;

        public PostController(IPostService postService)
        {
            _postService = postService;
        }

        [HttpGet]
        public async Task<ActionResult<ReadResponse<PageResult<PostInListResponse>>>> GetAllPosts(
            [FromQuery] GetAllPostsRequest request
        )
        {
            var currentUserId = User.GetUserId();
            var result = await _postService.GetAllPostsAsync(request, currentUserId);
            return ToActionResult(result);
        }
    }
}
