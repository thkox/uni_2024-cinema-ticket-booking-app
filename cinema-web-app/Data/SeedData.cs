using cinema_web_app.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace cinema_web_app.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            string[] roleNames = { "ApplicationAdmin", "ContentCinemaAdmin", "ContentAppAdmin", "Customer" };
            IdentityResult roleResult;

            // Create roles if they don't already exist
            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    roleResult = await roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
                    if (!roleResult.Succeeded)
                    {
                        throw new Exception($"Failed to create role '{roleName}'. Errors: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
                    }
                }
            }

            // Create Admin user and assign roles
            var adminEmail = "admin@example.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "Admin",
                    LastName = "User"
                };
                var adminPassword = "Admin@123"; // Ideally, use environment variables or secure vaults for sensitive data
                var adminCreateResult = await userManager.CreateAsync(adminUser, adminPassword);
                if (adminCreateResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "ApplicationAdmin");
                }
                else
                {
                    throw new Exception($"Failed to create Admin user. Errors: {string.Join(", ", adminCreateResult.Errors.Select(e => e.Description))}");
                }
            }

            // Create Customer user and assign roles
            var customerEmail = "customer@example.com";
            var customerUser = await userManager.FindByEmailAsync(customerEmail);
            if (customerUser == null)
            {
                customerUser = new ApplicationUser
                {
                    UserName = customerEmail,
                    Email = customerEmail,
                    FirstName = "Customer",
                    LastName = "User"
                };
                var customerPassword = "Customer@123"; // Ideally, use environment variables or secure vaults for sensitive data
                var customerCreateResult = await userManager.CreateAsync(customerUser, customerPassword);
                if (customerCreateResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(customerUser, "Customer");
                }
                else
                {
                    throw new Exception($"Failed to create Customer user. Errors: {string.Join(", ", customerCreateResult.Errors.Select(e => e.Description))}");
                }
            }
        }
    }
}
