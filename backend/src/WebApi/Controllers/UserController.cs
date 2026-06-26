using Application.DTOs;
using Application.DTOs.Request;
using Application.DTOs.Response;
using Application.Services;
using Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Extentions;

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
        public async Task<ActionResult<ReadResponseDto<PageResult<UserListItemDto>>>> GetAllUsers(
            [FromQuery] string? keyWord,
            [FromQuery] int currentPage = 1,
            [FromQuery] int pageSize = 10
        )
        {
            var result = await _userService.GetAllAsync(keyWord, currentPage, pageSize);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ReadResponseDto<UserDto>>> GetUserById([FromRoute] Guid id)
        {
            var result = await _userService.GetByIdAsync(id);
            if (!result.IsSuccess)
                return NotFound(result);

            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<WriteResponseDto>> CreateUser(
            [FromBody] CreateUserRequestDto request
        )
        {
            var result = await _userService.CreateAsync(request);
            if (!result.IsSuccess)
                return BadRequest(result);
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<WriteResponseDto>> UpdateUser(
            [FromRoute] Guid id,
            [FromBody] UpdateUserRequestDto? request
        )
        {
            if (request == null)
            {
                return BadRequest(
                    new WriteResponseDto
                    {
                        IsSuccess = false,
                        ErrorCode = Application.Constants.ErrorMessages.Common.InvalidRequest,
                        ErrorMessage = Application.Constants.ErrorMessages.Common.InvalidRequest,
                    }
                );
            }

            var result = await _userService.UpdateAsync(id, request);
            if (!result.IsSuccess)
            {
                return result.ErrorCode == Application.Constants.ErrorMessages.User.UserNotFound
                    ? NotFound(result)
                    : BadRequest(result);
            }
            return Ok(result);
        }

        [HttpDelete]
        public async Task<ActionResult<WriteResponseDto>> DeleteUsers([FromBody] List<Guid> ids)
        {
            var result = await _userService.DeleteAsync(ids);
            if (!result.IsSuccess)
            {
                return result.ErrorCode == Application.Constants.ErrorMessages.User.UsersNotFound
                    ? NotFound(result)
                    : BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPut("change-password")]
        public async Task<ActionResult<WriteResponseDto>> ChangeMyPassword(
            [FromBody] ChangeMyPasswordRequest request
        )
        {
            var result = await _userService.ChangeMyPasswordAsync(User.GetUserId(), request);
            if (!result.IsSuccess)
            {
                return
                    result.ErrorCode == Application.Constants.ErrorMessages.User.UserNotFound
                    || result.ErrorCode == Application.Constants.ErrorMessages.User.UsersNotFound
                    ? NotFound(result)
                    : BadRequest(result);
            }
            return Ok(result);
        }
    }
}
