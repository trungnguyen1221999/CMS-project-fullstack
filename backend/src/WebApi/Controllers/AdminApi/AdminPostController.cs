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
    public class AdminPostController : ControllerBase
    {
        private readonly IAdminPostService _postService;

        public AdminPostController(IAdminPostService postService)
        {
            _postService = postService;
        }

        //Read
        [HttpGet]
        [HasPermission(Permissions.Posts.View)]
        public async Task<ActionResult> GetAllPosts(
            [FromQuery] GetAllPostsRequest request
        )
        {
            var currentUserId = User.GetUserId();
            var result = await _postService.GetAllPostsAsync(request, currentUserId);
            return Ok(result);
        }

        [HttpGet("{postId}")]
        [HasPermission(Permissions.Posts.View)]
        public async Task<ActionResult> GetPostById([FromRoute] Guid postId)
        {
            var currentUserId = User.GetUserId();
            var result = await _postService.GetPostByIdAsync(postId, currentUserId);
            return Ok(result);
        }

        [HttpGet("reject-reason/{postId}")]
        [HasPermission(Permissions.Posts.View)]
        public async Task<ActionResult> GetRejectReason(
            [FromRoute] Guid postId
        )
        {
            var userId = User.GetUserId();
            var result = await _postService.GetRejectReasonAsync(postId, userId);
            return Ok(result);
        }

        [HttpGet("activity-logs/{postId}")]
        [Authorize(Posts.Approve)]
        public async Task<ActionResult> GetActivityLogs(
            [FromRoute] Guid postId
        )
        {
            var userId = User.GetUserId();
            var result = await _postService.GetActivityLogsAsync(postId, userId);
            return Ok(result);
        }

        //Write
        [HttpPost]
        [HasPermission(Permissions.Posts.Create)]
        public async Task<ActionResult> CreatePost(
            [FromBody] CreateUpdatePostRequest request
        )
        {
            var currentUserId = User.GetUserId();
            await _postService.CreatePostAsync(request, currentUserId);
            return Ok(WriteResponse.Success());
        }

        [HttpPut("{postId}")]
        [HasPermission(Permissions.Posts.Edit)]
        public async Task<ActionResult> EditPost(
            [FromBody] CreateUpdatePostRequest request,
            [FromRoute] Guid postId
        )
        {
            var currentUserId = User.GetUserId();
            await _postService.UpdatePostAsync(request, postId, currentUserId);
            return Ok(WriteResponse.Success());
        }

        [HttpDelete]
        [HasPermission(Permissions.Posts.Delete)]
        public async Task<ActionResult> DeletePost([FromQuery] Guid[] ids)
        {
            var currentUserId = User.GetUserId();
            await _postService.DeletePostAsync(ids, currentUserId);
            return Ok(WriteResponse.Success());
        }

        [HttpPut("approve/{postId}")]
        [HasPermission(Permissions.Posts.Approve)]
        public async Task<ActionResult> ApprovePost(
            [FromRoute] Guid postId,
            [FromBody] string? note
        )
        {
            var currentUserId = User.GetUserId();
            await _postService.ApprovePostAsync(postId, currentUserId, note);
            return Ok(WriteResponse.Success());
        }

        [HttpPut("reject/{postId}")]
        [HasPermission(Permissions.Posts.Approve)]
        public async Task<ActionResult> RejectPost(
            [FromRoute] Guid postId,
            [FromBody] string? note
        )
        {
            var currentUserId = User.GetUserId();
            await _postService.RejectPostAsync(postId, currentUserId, note);
            return Ok(WriteResponse.Success());
        }

        [HttpPut("approval-submit/{postId}")]
        public async Task<ActionResult> SubmitPostForApproval(
            [FromRoute] Guid postId,
            [FromBody] string? note
        )
        {
            var currentUserId = User.GetUserId();
            await _postService.SubmitPostForApprovalAsync(postId, currentUserId, note);
            return Ok(WriteResponse.Success());
        }
    }
}
