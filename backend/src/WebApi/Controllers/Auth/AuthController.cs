using Application.Contracts.Auth.Requests;
using Application.Contracts.Auth.Responses;
using Application.Contracts.Common;
using Application.Contracts.Users.Requests;
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
        private readonly IForgotPasswordService _forgotPasswordService;

        public AuthController(
            ISignUpService signUpService,
            ISignInService signInService,
            IForgotPasswordService forgotPasswordService
        )
        {
            _signUpService = signUpService;
            _signInService = signInService;
            _forgotPasswordService = forgotPasswordService;
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

        [HttpPost("forgot-password")]
        public async Task<ActionResult<WriteResponse>> ForgotPassword(
            [FromBody] ForgotPasswordRequest request
        )
        {
            var result = await _forgotPasswordService.ForgotPasswordAsync(request);
            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("reset-password")]
        public async Task<ActionResult<WriteResponse>> ResetPassword(
            [FromBody] ResetPasswordRequest request
        )
        {
            var result = await _forgotPasswordService.ResetPasswordAsync(request);
            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }
    }
}
