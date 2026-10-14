#nullable enable

using AcxiomCRM.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AcxiomCRM.Data;

/// <summary>
/// Provides seeding logic for initial CRM database roles and administrator account.
/// </summary>
public static class DbInitializer
{
    private static readonly string[] Roles = ["Admin", "Manager", "SalesExecutive"];

    /// <summary>
    /// Initializes default identity roles and the default administrator user if none exist.
    /// </summary>
    /// <param name="serviceProvider">The application service provider.</param>
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        var logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger("AcxiomCRM.Data.DbInitializer");
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var configuration = serviceProvider.GetService<IConfiguration>();

        // 1. Seed Roles
        foreach (var role in Roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var roleResult = await roleManager.CreateAsync(new IdentityRole(role));
                if (roleResult.Succeeded)
                {
                    logger?.LogInformation("Created role: {Role}", role);
                }
                else
                {
                    var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                    logger?.LogError("Failed to create role {Role}: {Errors}", role, errors);
                }
            }
        }

        // 2. Seed Default Admin if no users exist
        if (!userManager.Users.Any())
        {
            var adminEmail = configuration?["AdminSettings:Email"];
            if (string.IsNullOrWhiteSpace(adminEmail))
            {
                adminEmail = "admin@acxiomcrm.com";
            }

            var adminPassword = configuration?["AdminSettings:Password"];
            if (string.IsNullOrWhiteSpace(adminPassword))
            {
                adminPassword = "Admin@123456";
            }

            var adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "System",
                LastName = "Administrator",
                EmailConfirmed = true,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            var userResult = await userManager.CreateAsync(adminUser, adminPassword);
            if (userResult.Succeeded)
            {
                logger?.LogInformation("Created default administrator user: {Email}", adminEmail);

                var roleAssignResult = await userManager.AddToRoleAsync(adminUser, "Admin");
                if (roleAssignResult.Succeeded)
                {
                    logger?.LogInformation("Assigned Admin role to: {Email}", adminEmail);
                }
                else
                {
                    var errors = string.Join(", ", roleAssignResult.Errors.Select(e => e.Description));
                    logger?.LogError("Failed to assign Admin role to {Email}: {Errors}", adminEmail, errors);
                }
            }
            else
            {
                var errors = string.Join(", ", userResult.Errors.Select(e => e.Description));
                logger?.LogError("Failed to create admin user {Email}: {Errors}", adminEmail, errors);
            }
        }
    }
}
