using BlogProject.Core.Domain.Identity;
using Infrastructure;
using Microsoft.AspNetCore.Identity;

namespace WebApi.Extentions
{
    public static class IdentityConfig
    {
        public static WebApplicationBuilder ConfigureIdentity(this WebApplicationBuilder builder)
        {
            builder
                .Services.AddIdentity<User, Role>(options =>
                {
                    options.Password.RequireDigit = true;
                    options.Password.RequireLowercase = false;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireUppercase = false;
                    options.Password.RequiredLength = 6;
                })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            return builder;
        }
    }
}
