using Application.Contracts.Auth.Requests;
using Application.Contracts.Common;
using Application.Contracts.Users.Requests;
using Application.Services.Auth;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers.ClientApi
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
        public async Task<ActionResult> SignUp(
            [FromBody] SignUpRequest request
        )
        {
            await _signUpService.SignUpAsync(request);
            return Ok(WriteResponse.Success());
        }

        [HttpPost("signin")]
        public async Task<ActionResult> SignIn(
            [FromBody] SignInRequest request
        )
        {
            var result = await _signInService.SignInAsync(request);
            return Ok(result);
        }

        [HttpPost("forgot-password")]
        public async Task<ActionResult> ForgotPassword(
            [FromBody] ForgotPasswordRequest request
        )
        {
            await _forgotPasswordService.ForgotPasswordAsync(request);
            return Ok(WriteResponse.Success());
        }

        [HttpPost("reset-password")]
        public async Task<ActionResult> ResetPassword(
            [FromBody] ResetPasswordRequest request
        )
        {
            await _forgotPasswordService.ResetPasswordAsync(request);
            return Ok(WriteResponse.Success());
        }
    }
}
