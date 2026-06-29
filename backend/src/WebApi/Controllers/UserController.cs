using Application.Constants;
using Application.Contracts.Common;
using Application.Contracts.Users.Requests;
using Application.Contracts.Users.Responses;
using Application.Services;
using Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Extensions;

namespace WebApi.Controllers
{
    [Route("api/users")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<ActionResult<ReadResponse<PageResult<UserListItemResponse>>>> GetAllUsers(
            [FromQuery] string? keyWord,
            [FromQuery] int currentPage = 1,
            [FromQuery] int pageSize = 10
        )
        {
            var result = await _userService.GetAllAsync(keyWord, currentPage, pageSize);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ReadResponse<UserResponse>>> GetUserById([FromRoute] Guid id)
        {
            var result = await _userService.GetByIdAsync(id);
            return ToActionResult(result);
        }

        [HttpPost]
        public async Task<ActionResult<WriteResponse>> CreateUser(
            [FromBody] CreateUserRequest request
        )
        {
            var result = await _userService.CreateAsync(request);
            return ToActionResult(result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<WriteResponse>> UpdateUser(
            [FromRoute] Guid id,
            [FromBody] UpdateUserRequest? request
        )
        {
            if (request == null)
                return BadRequest(WriteResponse.Failure(ErrorMessages.Common.InvalidRequest));

            var result = await _userService.UpdateAsync(id, request);
            return ToActionResult(result);
        }

        [HttpDelete]
        public async Task<ActionResult<WriteResponse>> DeleteUsers([FromBody] List<Guid> ids)
        {
            var result = await _userService.DeleteAsync(ids);
            return ToActionResult(result);
        }

        [HttpPut("change-password")]
        public async Task<ActionResult<WriteResponse>> ChangeMyPassword(
            [FromBody] ChangeMyPasswordRequest request
        )
        {
            var userId = User.GetUserId();
            var result = await _userService.ChangeMyPasswordAsync(userId, request);
            return ToActionResult(result);
        }

        [HttpPut("{id}/set-password")]
        public async Task<ActionResult<WriteResponse>> SetPassword(
            [FromRoute] Guid id,
            [FromBody] SetPasswordRequest request
        )
        {
            var result = await _userService.SetPasswordAsync(id, request);
            return ToActionResult(result);
        }

        [HttpPut("{id}/change-email")]
        public async Task<ActionResult<WriteResponse>> ChangeEmail(
            [FromRoute] Guid id,
            [FromBody] ChangeEmailRequest request
        )
        {
            var result = await _userService.ChangeEmailAsync(id, request);
            return ToActionResult(result);
        }

        private ActionResult ToActionResult(WriteResponse result)
        {
            if (result.IsSuccess)
                return Ok(result);

            return IsNotFoundError(result.ErrorCode)
                ? NotFound(result)
                : BadRequest(result);
        }

        private static bool IsNotFoundError(string? errorCode) =>
            errorCode is ErrorMessages.User.UserNotFound
                or ErrorMessages.User.UsersNotFound;
    }
}
