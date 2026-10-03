using Library.Api.Security;
using Microsoft.AspNetCore.Identity;

namespace Library.Api.Data;

public static class IdentitySeeder
{
    public static async Task SeedAsync(
        RoleManager<IdentityRole> roleManager,
        UserManager<IdentityUser> userManager)
    {
        string[] roleNames =
        [
            RoleNames.Admin,
            RoleNames.Borrower
        ];

        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var result = await roleManager.CreateAsync(
                    new IdentityRole(roleName));

                if (!result.Succeeded)
                {
                    throw new InvalidOperationException(
                        $"Failed to create role '{roleName}': " +
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
        }

        var adminUserName = "admin";

        var adminUser = await userManager.FindByNameAsync(adminUserName);

        if (adminUser is null)
        {
            adminUser = new IdentityUser
            {
                UserName = adminUserName
            };

            var createResult = await userManager.CreateAsync(
                adminUser,
                "Admin123!");

            if (!createResult.Succeeded)
            {
                throw new InvalidOperationException(
                    "Failed to create admin user: " +
                    string.Join(", ",
                        createResult.Errors.Select(e => e.Description)));
            }

            var roleResult = await userManager.AddToRoleAsync(
                adminUser,
                RoleNames.Admin);

            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException(
                    "Failed to add admin user to Admin role: " +
                    string.Join(", ",
                        roleResult.Errors.Select(e => e.Description)));
            }
        }

        var borrowerUserName = "borrower";

        var borrowerUser =
            await userManager.FindByNameAsync(borrowerUserName);

        if (borrowerUser is null)
        {
            borrowerUser = new IdentityUser
            {
                UserName = borrowerUserName
            };

            var createResult = await userManager.CreateAsync(
                borrowerUser,
                "Borrower123!");

            if (!createResult.Succeeded)
            {
                throw new InvalidOperationException(
                    "Failed to create borrower user: " +
                    string.Join(", ",
                        createResult.Errors.Select(e => e.Description)));
            }

            var roleResult = await userManager.AddToRoleAsync(
                borrowerUser,
                RoleNames.Borrower);

            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException(
                    "Failed to add borrower user to Borrower role: " +
                    string.Join(", ",
                        roleResult.Errors.Select(e => e.Description)));
            }
        }
    }
}