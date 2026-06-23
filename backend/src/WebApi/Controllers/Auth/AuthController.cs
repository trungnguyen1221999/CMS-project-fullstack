using Application.DTOs.Request.Auth;
using Application.DTOs.Response.Auth;
using Application.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers.Auth
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ISignUpService _signUpService;

        public AuthController(ISignUpService signUpService)
        {
            _signUpService = signUpService;
        }

        [HttpPost("signup")]
        public async Task<ActionResult<SignUpResponseDto>> SignUp(
            [FromBody] SignUpRequestDto request
        )
        {
            var result = await _signUpService.SignUpAsync(request);
            if (!result.IsSuccess)
            {
                return BadRequest(
                    new SignUpResponseDto { IsSuccess = false, ErrorMessage = result.ErrorMessage }
                );
            }
            return Ok(new SignUpResponseDto { IsSuccess = true });
        }
    }
}
