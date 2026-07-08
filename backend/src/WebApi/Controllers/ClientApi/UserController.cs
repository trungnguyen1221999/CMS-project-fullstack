using Application.Contracts.Common;
using Application.Contracts.Users.Requests;
using Application.Services.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Extensions;

namespace WebApi.Controllers.ClientApi
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

        [HttpPut("change-password")]
        public async Task<ActionResult> ChangeMyPassword(
            [FromBody] ChangeMyPasswordRequest request
        )
        {
            var userId = User.GetUserId();
            await _userService.ChangeMyPasswordAsync(userId, request);
            return Ok(WriteResponse.Success());
        }
    }
}
