using Application.Constants;
using Application.Contracts.Common;
using Application.Contracts.Users.Requests;
using Application.Contracts.Users.Responses;
using Application.Services.User;
using Domain;
using Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Authorization;

namespace WebApi.Controllers.AdminApi
{
    [Route("api/admin/users")]
    [ApiController]
    [Authorize]
    public class AdminUserController : ApiControllerBase
    {
        private readonly IUserService _userService;

        public AdminUserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        [HasPermission(Permissions.Users.View)]
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
        [HasPermission(Permissions.Users.View)]
        public async Task<ActionResult<ReadResponse<UserResponse>>> GetUserById([FromRoute] Guid id)
        {
            var result = await _userService.GetByIdAsync(id);
            return ToActionResult(result);
        }

        [HttpPost]
        [HasPermission(Permissions.Users.Create)]
        public async Task<ActionResult<WriteResponse>> CreateUser(
            [FromBody] CreateUserRequest request
        )
        {
            var result = await _userService.CreateAsync(request);
            return ToActionResult(result);
        }

        [HttpPut("{id}")]
        [HasPermission(Permissions.Users.Edit)]
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
        [HasPermission(Permissions.Users.Delete)]
        public async Task<ActionResult<WriteResponse>> DeleteUsers([FromBody] List<Guid> ids)
        {
            var result = await _userService.DeleteAsync(ids);
            return ToActionResult(result);
        }

        [HttpPut("{id}/set-password")]
        [HasPermission(Permissions.Users.Edit)]
        public async Task<ActionResult<WriteResponse>> SetPassword(
            [FromRoute] Guid id,
            [FromBody] SetPasswordRequest request
        )
        {
            var result = await _userService.SetPasswordAsync(id, request);
            return ToActionResult(result);
        }

        [HttpPut("{id}/change-email")]
        [HasPermission(Permissions.Users.Edit)]
        public async Task<ActionResult<WriteResponse>> ChangeEmail(
            [FromRoute] Guid id,
            [FromBody] ChangeEmailRequest request
        )
        {
            var result = await _userService.ChangeEmailAsync(id, request);
            return ToActionResult(result);
        }

        [HttpPut("{id}/assign-roles")]
        [HasPermission(Permissions.Users.Edit)]
        public async Task<ActionResult<WriteResponse>> AssignRolesToUser(
            [FromRoute] Guid id,
            [FromBody] string[] roles
        )
        {
            var result = await _userService.AssignRolesToUserAsync(id, roles);
            return ToActionResult(result);
        }
    }
}
