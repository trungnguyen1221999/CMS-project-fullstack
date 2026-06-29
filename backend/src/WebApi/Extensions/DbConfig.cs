using Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace WebApi.Extensions
{
    public static class DbConfig
    {
        public static WebApplicationBuilder ConfigureDatabase(
            this WebApplicationBuilder builder,
            string connectionStringName
        )
        {
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(
                    builder.Configuration.GetConnectionString(connectionStringName),
                    b => b.MigrationsAssembly("Infrastructure")
                )
            );

            return builder;
        }
    }
}
