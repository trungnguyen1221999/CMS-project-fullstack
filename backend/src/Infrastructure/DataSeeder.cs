using System.Security.Claims;
using Domain.Constants;
using Domain.Cores.Identity;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure;

public static class DataSeeder
{
    public static async Task SeedEverythingAsync(
        UserManager<User> userManager,
        RoleManager<Role> roleManager
    )
    {
        // 1. COMBINE BASE PERMISSIONS FOR EACH CMS ROLE
        // Prepare HashSet containers to aggregate permissions cleanly without duplication

        // Author = Public Reader + Writing Permissions
        var authorPermissions = new HashSet<string>(RolePermissions.PublicReader);
        authorPermissions.UnionWith(RolePermissions.Writing);

        // Editor = Public Reader + Editing/Publishing Permissions (No write permissions assigned)
        var editorPermissions = new HashSet<string>(RolePermissions.PublicReader);
        editorPermissions.UnionWith(RolePermissions.Editing);

        // Admin = Full system access (Uses reflection extension to fetch all defined constants)
        var adminPermissions = typeof(Permissions).GetAllPermissionValues().ToHashSet();

        // User = Public Reader permissions only
        var userPermissions = new HashSet<string>(RolePermissions.PublicReader);

        // 2. MAP ROLES TO THEIR AGGREGATED PERMISSION SETS
        var rolePermissionsMapping = new Dictionary<
            string,
            (string DisplayName, HashSet<string> AllowedPermissions)
        >
        {
            { Roles.Admin, ("System Administrator", adminPermissions) },
            { Roles.Editor, ("Content Editor", editorPermissions) },
            { Roles.Author, ("Content Writer / Creator", authorPermissions) },
            { Roles.User, ("Regular Member / Subscriber", userPermissions) },
        };

        // 3. LOOP THROUGH MAPPING TO CREATE ROLES AND ASSIGN CLAIMS
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

            // Fetch currently assigned claims for this role to avoid duplicate inserts
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

        // 4. SEED DEMO ACCOUNTS AND ATTACH ROLES
        await SeedUserAsync(
            userManager,
            "admin@gmail.com",
            "admin",
            "System",
            "Admin",
            Roles.Admin
        );
        await SeedUserAsync(
            userManager,
            "editor@gmail.com",
            "editor",
            "Kai",
            "Editor",
            Roles.Editor
        );
        await SeedUserAsync(
            userManager,
            "author@gmail.com",
            "author",
            "Nguyen",
            "Author",
            Roles.Author
        );
        await SeedUserAsync(userManager, "user@gmail.com", "user", "John", "Doe", Roles.User);
    }

    // Helper method to handle automated user registration and role embedding
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
                RoyaltyAmountPerPost = roleName == Roles.Author ? 15 : 0, // Default token rate for authors
            };

            // Identity automatically intercept this string and hashes it safely into the DB
            var result = await userManager.CreateAsync(newUser, "123456");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(newUser, roleName);
            }
        }
    }
}
