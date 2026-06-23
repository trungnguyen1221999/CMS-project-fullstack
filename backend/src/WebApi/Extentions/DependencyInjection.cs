using Application.Services.Auth;
using Application.Services.Token;
using Infrastructure.Services.Auth;
using Infrastructure.Services.Token;
using WebApi.Controllers.Auth;

namespace WebApi.Extentions
{
    public static class DependencyInjection
    {
        public static WebApplicationBuilder AddDI(this WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<AuthController>();
            builder.Services.AddScoped<ISignUpService, SignUpService>();
            builder.Services.AddScoped<ITokenService, TokenService>();
            builder.Services.AddScoped<ISignInService, SignInService>();

            // Add your service registrations here

            return builder;
        }
    }
}
