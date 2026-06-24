using Application.DTOs;
using Application.DTOs.Request;
using Application.DTOs.Response;
using Application.Services;
using Domain.Cores.Identity;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers()
        {
            var result = await _userService.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUserById(Guid userId)
        {
            var result = await _userService.GetByIdAsync(userId);
            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<ResponseDto<User>>> CreateUser(
            [FromBody] CreateUserRequestDto request
        )
        {
            var result = await _userService.CreateAsync(request);
            if (!result.IsSuccess)
                return BadRequest(result.ErrorMessage);
            return Ok(result);
        }
    }
}
