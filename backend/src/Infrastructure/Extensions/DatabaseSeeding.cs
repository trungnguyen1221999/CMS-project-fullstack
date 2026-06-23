using Domain.Cores.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Extensions
{
    public static class DatabaseSeeding
    {
        public static async Task MigrateAndSeedDatabaseAsync(this IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var services = scope.ServiceProvider;

            try
            {
                // 1. Automatically run migrations
                var context = services.GetRequiredService<ApplicationDbContext>();
                await context.Database.MigrateAsync();

                // 2. Get Identity managers
                var userManager = services.GetRequiredService<UserManager<User>>();
                var roleManager = services.GetRequiredService<RoleManager<Role>>();

                // 3. Trigger the main data seeder
                await DataSeeder.SeedEverythingAsync(userManager, roleManager);
                Console.WriteLine("----> CMS Database Migration & Seeding completed successfully!");
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<ApplicationDbContext>>();
                logger.LogError(ex, "An error occurred while migrating or seeding the database.");
                throw;
            }
        }
    }
}
