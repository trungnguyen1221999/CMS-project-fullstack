using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using WebApi.Authorization;

namespace WebApi.Extensions
{
    public static class AuthConfig
    {
        public static WebApplicationBuilder ConfigureAuth(this WebApplicationBuilder builder)
        {
            var jwtSettings = builder.Configuration.GetSection("JwtSettings");
            var secret =
                jwtSettings["Secret"]
                ?? throw new InvalidOperationException("Missing JwtSettings:Secret");
            var issuer =
                jwtSettings["Issuer"]
                ?? throw new InvalidOperationException("Missing JwtSettings:Issuer");
            var audience =
                jwtSettings["Audience"]
                ?? throw new InvalidOperationException("Missing JwtSettings:Audience");

            builder
                .Services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = issuer,
                        ValidAudience = audience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                        ClockSkew = TimeSpan.Zero,
                    };
                });

            builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
            builder.Services.AddSingleton<IAuthorizationHandler, PermissionHandler>();
            builder.Services.AddAuthorization();

            return builder;
        }
    }
}
