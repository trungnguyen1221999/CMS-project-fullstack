using Application.Contracts.Common;
using Application.Contracts.Posts.Request;
using Application.Contracts.Posts.Response;
using Application.Services.Post;
using Domain;
using Domain.Constants;
using Domain.Cores.Content;
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

        //Read
        [HttpGet]
        [HasPermission(Permissions.Posts.View)]
        public async Task<ActionResult<ReadResponse<PageResult<PostInListResponse>>>> GetAllPosts(
            [FromQuery] GetAllPostsRequest request
        )
        {
            var currentUserId = User.GetUserId();
            var result = await _postService.AdminGetAllPostsAsync(request, currentUserId);
            return ToActionResult(result);
        }

        [HttpGet("{postId}")]
        [HasPermission(Permissions.Posts.View)]
        public async Task<ActionResult<ReadResponse<Post>>> GetPostById([FromRoute] Guid postId)
        {
            var currentUserId = User.GetUserId();
            var result = await _postService.AdminGetPostByIdAsync(postId, currentUserId);
            return ToActionResult(result);
        }

        //Write
        [HttpPost]
        [HasPermission(Permissions.Posts.Create)]
        public async Task<ActionResult<WriteResponse>> CreatePost(
            [FromBody] CreateUpdatePostRequest request
        )
        {
            var currentUserId = User.GetUserId();
            var result = await _postService.AdminCreatePostAsync(request, currentUserId);
            return ToActionResult(result);
        }

        [HttpPut("{postId}")]
        [HasPermission(Permissions.Posts.Edit)]
        public async Task<ActionResult<WriteResponse>> EditPost(
            [FromBody] CreateUpdatePostRequest request,
            [FromRoute] Guid postId
        )
        {
            var currentUserId = User.GetUserId();
            var result = await _postService.AdminUpdatePostAsync(request, postId, currentUserId);
            return ToActionResult(result);
        }

        [HttpDelete]
        [HasPermission(Permissions.Posts.Delete)]
        public async Task<ActionResult<WriteResponse>> DeletePost([FromQuery] Guid[] ids)
        {
            var currentUserId = User.GetUserId();
            var result = await _postService.AdminDeletePostAsync(ids, currentUserId);
            return ToActionResult(result);
        }
    }
}
