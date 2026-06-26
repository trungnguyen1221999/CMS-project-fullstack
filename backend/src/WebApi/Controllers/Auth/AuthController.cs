using Application.Contracts.Auth.Requests;
using Application.Contracts.Auth.Responses;
using Application.Services.Auth;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers.Auth
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ISignUpService _signUpService;
        private readonly ISignInService _signInService;

        public AuthController(ISignUpService signUpService, ISignInService signInService)
        {
            _signUpService = signUpService;
            _signInService = signInService;
        }

        [HttpPost("signup")]
        public async Task<ActionResult<SignUpResponse>> SignUp(
            [FromBody] SignUpRequest request
        )
        {
            var result = await _signUpService.SignUpAsync(request);
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPost("signin")]
        public async Task<ActionResult<SignInResponse>> SignIn(
            [FromBody] SignInRequest request
        )
        {
            var result = await _signInService.SignInAsync(request);
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}
