using Application.Services;
using Infrastructure.Services.Auth;
using WebApi.Controllers.Auth;

namespace WebApi.Extentions
{
    public static class DependencyInjection
    {
        public static WebApplicationBuilder AddDI(this WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<AuthController>();
            builder.Services.AddScoped<ISignUpService, SignUpService>();

            // Add your service registrations here

            return builder;
        }
    }
}
