using cinema_web_app.Models;
using Microsoft.AspNetCore.Identity;

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

            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    // Create the roles and seed them to the database
                    roleResult = await roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
                }
            }

            // Create Admin user and assign roles
            ApplicationUser adminUser = await userManager.FindByEmailAsync("admin@example.com");
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = "admin@example.com",
                    Email = "admin@example.com",
                    FirstName = "Admin",
                    LastName = "User"
                };
                await userManager.CreateAsync(adminUser, "Admin@123");

                // Assign Admin role
                await userManager.AddToRoleAsync(adminUser, "ApplicationAdmin");
            }

            // Create Customer user and assign roles
            ApplicationUser customerUser = await userManager.FindByEmailAsync("customer@example.com");
            if (customerUser == null)
            {
                customerUser = new ApplicationUser
                {
                    UserName = "customer@example.com",
                    Email = "customer@example.com",
                    FirstName = "Customer",
                    LastName = "User"
                };
                await userManager.CreateAsync(customerUser, "Customer@123");

                // Assign Customer role
                await userManager.AddToRoleAsync(customerUser, "Customer");
            }
        }
    }
}
