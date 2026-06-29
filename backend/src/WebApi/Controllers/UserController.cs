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
            if (!result.IsSuccess)
                return NotFound(result);

            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<WriteResponse>> CreateUser(
            [FromBody] CreateUserRequest request
        )
        {
            var result = await _userService.CreateAsync(request);
            if (!result.IsSuccess)
                return BadRequest(result);
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<WriteResponse>> UpdateUser(
            [FromRoute] Guid id,
            [FromBody] UpdateUserRequest? request
        )
        {
            if (request == null)
            {
                return BadRequest(
                    new WriteResponse
                    {
                        IsSuccess = false,
                        ErrorCode = ErrorMessages.Common.InvalidRequest,
                        ErrorMessage = ErrorMessages.Common.InvalidRequest,
                    }
                );
            }

            var result = await _userService.UpdateAsync(id, request);
            if (!result.IsSuccess)
            {
                return result.ErrorCode == ErrorMessages.User.UserNotFound
                    ? NotFound(result)
                    : BadRequest(result);
            }
            return Ok(result);
        }

        [HttpDelete]
        public async Task<ActionResult<WriteResponse>> DeleteUsers([FromBody] List<Guid> ids)
        {
            var result = await _userService.DeleteAsync(ids);
            if (!result.IsSuccess)
            {
                return result.ErrorCode == ErrorMessages.User.UsersNotFound
                    ? NotFound(result)
                    : BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPut("change-password")]
        public async Task<ActionResult<WriteResponse>> ChangeMyPassword(
            [FromBody] ChangeMyPasswordRequest request
        )
        {
            var result = await _userService.ChangeMyPasswordAsync(User.GetUserId(), request);
            if (!result.IsSuccess)
            {
                return
                    result.ErrorCode == ErrorMessages.User.UserNotFound
                    || result.ErrorCode == ErrorMessages.User.UsersNotFound
                    ? NotFound(result)
                    : BadRequest(result);
            }
            return Ok(result);
        }
    }
}
