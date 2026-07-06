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

namespace WebApi.Controllers.AdminApi
{
    [Route("api/admin/posts")]
    [ApiController]
    [Authorize]
    public class AdminPostController : ApiControllerBase
    {
        private readonly IPostService _postService;

        public AdminPostController(IPostService postService)
        {
            _postService = postService;
        }

        [HttpGet]
        [HasPermission(Permissions.Posts.View)]
        public async Task<ActionResult<ReadResponse<PageResult<PostInListResponse>>>> GetAllPosts(
            [FromQuery] GetAllPostsRequest request
        )
        {
            var currentUserId = User.GetUserId();
            var result = await _postService.GetAllPostsAsync(request, currentUserId);
            return ToActionResult(result);
        }

        [HttpPost]
        [HasPermission(Permissions.Posts.Create)]
        public async Task<ActionResult<WriteResponse>> CreatePost(
            [FromBody] CreateUpdatePostRequest request
        )
        {
            var currentUserId = User.GetUserId();
            var result = await _postService.CreatePostAsync(request, currentUserId);
            return ToActionResult(result);
        }
    }
}
