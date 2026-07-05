using Application.Contracts.Users.Responses;
using Application.Services.Auth;
using Application.Services.Otp;
using Application.Services.Permission;
using Application.Services.Post;
using Application.Services.Token;
using Application.Services.User;
using Application.UnitOfWork;
using Infrastructure.Services.Otp;
using Infrastructure.Services.Token;

namespace WebApi.Extensions
{
    public static class DependencyInjection
    {
        public static WebApplicationBuilder AddDI(this WebApplicationBuilder builder)
        {
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

            builder.Services.AddAutoMapper(cfg => cfg.AddMaps(typeof(UserResponse).Assembly));

            builder.Services.AddScoped<ISignUpService, SignUpService>();
            builder.Services.AddScoped<ITokenService, TokenService>();
            builder.Services.AddScoped<ISignInService, SignInService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<IForgotPasswordService, ForgotPasswordService>();
            builder.Services.AddScoped<IPermissionService, PermissionService>();
            builder.Services.AddScoped<IPostService, PostService>();

            return builder;
        }
    }
}
