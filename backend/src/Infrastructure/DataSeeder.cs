using System.Security.Claims;
using BlogProject.Core.Domain.Identity;
using Domain.Constants;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure;

public static class DataSeeder
{
    public static async Task SeedEverythingAsync(
        UserManager<User> userManager,
        RoleManager<Role> roleManager
    )
    {
        // 1. SEED ROLES & PERMISSIONS USING HASHSET FOR OPTIMIZED PERFORMANCE
        var rolePermissionsMapping = new Dictionary<
            string,
            (string DisplayName, HashSet<string> AllowedPermissions)
        >
        {
            {
                Roles.Admin,
                (
                    "System Administrator", // Superuser with full control over the entire CMS
                    new HashSet<string>
                    {
                        Permissions.Dashboard.View,
                        Permissions.Roles.View,
                        Permissions.Roles.Create,
                        Permissions.Roles.Edit,
                        Permissions.Roles.Delete,
                        Permissions.Users.View,
                        Permissions.Users.Create,
                        Permissions.Users.Edit,
                        Permissions.Users.Delete,
                        Permissions.PostCategories.View,
                        Permissions.PostCategories.Create,
                        Permissions.PostCategories.Edit,
                        Permissions.PostCategories.Delete,
                        Permissions.Posts.View,
                        Permissions.Posts.Create,
                        Permissions.Posts.Edit,
                        Permissions.Posts.Delete,
                        Permissions.Posts.Approve,
                        Permissions.Series.View,
                        Permissions.Series.Create,
                        Permissions.Series.Edit,
                        Permissions.Series.Delete,
                        Permissions.Royalty.View,
                        Permissions.Royalty.Pay,
                    }
                )
            },
            {
                Roles.Editor,
                (
                    "Content Editor", // Manages structure, reviews and approves/publishes content
                    new HashSet<string>
                    {
                        Permissions.Dashboard.View,
                        Permissions.PostCategories.View,
                        Permissions.PostCategories.Create,
                        Permissions.PostCategories.Edit,
                        Permissions.PostCategories.Delete,
                        Permissions.Posts.View,
                        Permissions.Posts.Edit,
                        Permissions.Posts.Delete,
                        Permissions.Posts.Approve,
                        Permissions.Series.View,
                        Permissions.Series.Create,
                        Permissions.Series.Edit,
                        Permissions.Series.Delete,
                        Permissions.Royalty.View,
                    }
                )
            },
            {
                Roles.Author,
                (
                    "Content Writer / Creator", // Creates and updates their own posts, submits them for review, views earned royalty
                    new HashSet<string>
                    {
                        Permissions.Posts.View,
                        Permissions.Posts.Create,
                        Permissions.Posts.Edit,
                        Permissions.Royalty.View,
                    }
                )
            },
            {
                "User", // Assuming you might add Roles.User later, using string literal for now
                (
                    "Regular Member / Subscriber", // Public audience, granted read-only access to published content
                    new HashSet<string> { Permissions.Posts.View }
                )
            },
        };

        // Loop through the mapping to create roles and assign their permissions
        foreach (var mapping in rolePermissionsMapping)
        {
            var roleName = mapping.Key;
            var (displayName, allowedPermissions) = mapping.Value;

            // Check if the role already exists in the database
            var role = await roleManager.FindByNameAsync(roleName);
            if (role == null)
            {
                role = new Role
                {
                    Id = Guid.NewGuid(),
                    Name = roleName,
                    DisplayName = displayName,
                };
                await roleManager.CreateAsync(role);
            }

            var existingClaims = await roleManager.GetClaimsAsync(role);

            // Assign permissions to the role as Role Claims
            foreach (var permission in allowedPermissions)
            {
                if (!existingClaims.Any(c => c.Type == "Permission" && c.Value == permission))
                {
                    await roleManager.AddClaimAsync(role, new Claim("Permission", permission));
                }
            }
        }

        // 2. SEED USERS & ASSIGN TO RESPECTIVE ROLES
        await SeedUserAsync(userManager, "admin@cms.com", "admin", "System", "Admin", Roles.Admin);
        await SeedUserAsync(userManager, "editor@cms.com", "editor", "Kai", "Editor", Roles.Editor);
        await SeedUserAsync(
            userManager,
            "author@cms.com",
            "author",
            "Nguyen",
            "Author",
            Roles.Author
        );
        await SeedUserAsync(userManager, "user@cms.com", "user", "John", "Doe", Roles.User);
    }

    private static async Task SeedUserAsync(
        UserManager<User> userManager,
        string email,
        string username,
        string firstName,
        string lastName,
        string roleName
    )
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            var newUser = new User
            {
                Id = Guid.NewGuid(),
                UserName = username,
                Email = email,
                EmailConfirmed = true,
                FirstName = firstName,
                LastName = lastName,
                IsActive = true,
                DateCreated = DateTime.UtcNow,
                Balance = 0,
                RoyaltyAmountPerPost = roleName == Roles.Author ? 15 : 0, // 15 per post for authors as default
            };

            // Create user with a strong default password (Identity automatically hashes this)
            var result = await userManager.CreateAsync(newUser, "123456");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(newUser, roleName);
            }
        }
    }
}
