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
using static Domain.Constants.Permissions;

namespace WebApi.Controllers.AdminApi
{
    [Route("api/admin/posts")]
    [ApiController]
    [Authorize]
    public class AdminPostController : ApiControllerBase
    {
        private readonly IAdminPostService _postService;

        public AdminPostController(IAdminPostService postService)
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
            var result = await _postService.GetAllPostsAsync(request, currentUserId);
            return ToActionResult(result);
        }

        [HttpGet("{postId}")]
        [HasPermission(Permissions.Posts.View)]
        public async Task<ActionResult<ReadResponse<Post>>> GetPostById([FromRoute] Guid postId)
        {
            var currentUserId = User.GetUserId();
            var result = await _postService.GetPostByIdAsync(postId, currentUserId);
            return ToActionResult(result);
        }

        [HttpGet("reject-reason/{postId}")]
        [HasPermission(Permissions.Posts.View)]
        public async Task<ActionResult<ReadResponse<string>>> GetRejectReason(
            [FromRoute] Guid postId
        )
        {
            var userId = User.GetUserId();
            var result = await _postService.GetRejectReasonAsync(postId, userId);
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
            var result = await _postService.CreatePostAsync(request, currentUserId);
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
            var result = await _postService.UpdatePostAsync(request, postId, currentUserId);
            return ToActionResult(result);
        }

        [HttpDelete]
        [HasPermission(Permissions.Posts.Delete)]
        public async Task<ActionResult<WriteResponse>> DeletePost([FromQuery] Guid[] ids)
        {
            var currentUserId = User.GetUserId();
            var result = await _postService.DeletePostAsync(ids, currentUserId);
            return ToActionResult(result);
        }

        [HttpPut("approve/{postId}")]
        [HasPermission(Permissions.Posts.Approve)]
        public async Task<ActionResult<WriteResponse>> ApprovePost(
            [FromRoute] Guid postId,
            [FromBody] string? note
        )
        {
            var currentUserId = User.GetUserId();
            var result = await _postService.ApprovePostAsync(postId, currentUserId, note);
            return ToActionResult(result);
        }

        [HttpPut("reject/{postId}")]
        [HasPermission(Permissions.Posts.Approve)]
        public async Task<ActionResult<WriteResponse>> RejectPost(
            [FromRoute] Guid postId,
            [FromBody] string? note
        )
        {
            var currentUserId = User.GetUserId();
            var result = await _postService.RejectPostAsync(postId, currentUserId, note);
            return ToActionResult(result);
        }

        [HttpPut("approval-submit/{postId}")]
        public async Task<ActionResult<WriteResponse>> SubmitPostForApproval(
            [FromRoute] Guid postId,
            [FromBody] string? note
        )
        {
            var currentUserId = User.GetUserId();
            var result = await _postService.SubmitPostForApprovalAsync(
                postId,
                currentUserId,
                note
            );
            return ToActionResult(result);
        }
    }
}
