using Application.Constants;
using Application.Contracts.Common;
using Application.Contracts.Users.Requests;
using Application.Services.User;
using Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Authorization;

namespace WebApi.Controllers.AdminApi
{
    [Route("api/admin/users")]
    [ApiController]
    [Authorize]
    public class AdminUserController : ControllerBase
    {
        private readonly IUserService _userService;

        public AdminUserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("paging")]
        [HasPermission(Permissions.Users.View)]
        public async Task<ActionResult> GetAllUsers([FromQuery] PagingRequest request)
        {
            var result = await _userService.GetAllAsync(request);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [HasPermission(Permissions.Users.View)]
        public async Task<ActionResult> GetUserById([FromRoute] Guid id)
        {
            var result = await _userService.GetByIdAsync(id);
            return Ok(result);
        }

        [HttpPost]
        [HasPermission(Permissions.Users.Create)]
        public async Task<ActionResult> CreateUser([FromBody] CreateUserRequest request)
        {
            await _userService.CreateAsync(request);
            return Ok(WriteResponse.Success());
        }

        [HttpPut("{id}")]
        [HasPermission(Permissions.Users.Edit)]
        public async Task<ActionResult> UpdateUser(
            [FromRoute] Guid id,
            [FromBody] UpdateUserRequest? request
        )
        {
            if (request == null)
                return BadRequest(WriteResponse.Failure(ErrorMessages.Common.InvalidRequest));

            await _userService.UpdateAsync(id, request);
            return Ok(WriteResponse.Success());
        }

        [HttpDelete]
        [HasPermission(Permissions.Users.Delete)]
        public async Task<ActionResult> DeleteUsers([FromBody] List<Guid> ids)
        {
            await _userService.DeleteAsync(ids);
            return Ok(WriteResponse.Success());
        }

        [HttpPut("{id}/set-password")]
        [HasPermission(Permissions.Users.Edit)]
        public async Task<ActionResult> SetPassword(
            [FromRoute] Guid id,
            [FromBody] SetPasswordRequest request
        )
        {
            await _userService.SetPasswordAsync(id, request);
            return Ok(WriteResponse.Success());
        }

        [HttpPut("{id}/change-email")]
        [HasPermission(Permissions.Users.Edit)]
        public async Task<ActionResult> ChangeEmail(
            [FromRoute] Guid id,
            [FromBody] ChangeEmailRequest request
        )
        {
            await _userService.ChangeEmailAsync(id, request);
            return Ok(WriteResponse.Success());
        }

        [HttpPut("{id}/assign-roles")]
        [HasPermission(Permissions.Users.Edit)]
        public async Task<ActionResult> AssignRolesToUser(
            [FromRoute] Guid id,
            [FromBody] string[] roles
        )
        {
            await _userService.AssignRolesToUserAsync(id, roles);
            return Ok(WriteResponse.Success());
        }
    }
}
