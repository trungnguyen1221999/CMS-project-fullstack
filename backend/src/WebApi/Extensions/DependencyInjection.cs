using Application.Contracts.Users.Responses;
using Application.Services;
using Application.Services.Auth;
using Application.Services.Token;
using Application.UnitOfWork;
using Infrastructure.Services.Token;

namespace WebApi.Extensions
{
    public static class DependencyInjection
    {
        public static WebApplicationBuilder AddDI(this WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

            builder.Services.AddAutoMapper(cfg => cfg.AddMaps(typeof(UserResponse).Assembly));

            builder.Services.AddScoped<ISignUpService, SignUpService>();
            builder.Services.AddScoped<ITokenService, TokenService>();
            builder.Services.AddScoped<ISignInService, SignInService>();
            builder.Services.AddScoped<IUserService, UserService>();

            return builder;
        }
    }
}
