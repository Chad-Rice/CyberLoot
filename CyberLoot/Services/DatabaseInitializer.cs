using CyberLoot.Models;
using Microsoft.AspNetCore.Identity;

namespace CyberLoot.Services
{
    public class DatabaseInitializer
    {
        public static async Task SeedDataAsync(UserManager<ApplicationUser>? userManager, 
            RoleManager<IdentityRole>? roleManager)
        {
            if (userManager == null || roleManager == null)
            {
                Console.WriteLine("userManager or roleManager is null => exit");
                return;
            }

            // check if there is an admin role or not
            var exists = await roleManager.RoleExistsAsync("admin");
            if (!exists)
            {
                Console.WriteLine("Admin role isn't defined and will be created");
                await roleManager.CreateAsync(new IdentityRole("admin"));
            }

            // check if there is a publisher role or not
            exists = await roleManager.RoleExistsAsync("publisher");
            if (!exists)
            {
                Console.WriteLine("Publisher role isn't defined and will be created");
                await roleManager.CreateAsync(new IdentityRole("publisher"));
            }

            // check if there is a client role or not
            exists = await roleManager.RoleExistsAsync("client");
            if (!exists)
            {
                Console.WriteLine("Client role isn't defined and will be created");
                await roleManager.CreateAsync(new IdentityRole("client"));
            }

            // check if there is at least one admin user or not
            var adminUsers = await userManager.GetUsersInRoleAsync("admin");
            if (adminUsers.Any())
            {
                Console.WriteLine("Admin user already exists => exit");
                return;
            }

            // create admin user
            var user = new ApplicationUser()
            {
                FirstName = "Admin",
                LastName = "Admin",
                UserName = "admin@admin.com",   // Will be used to authenticate the user
                Email = "admin@admin.com",
                JoinedOn = DateTime.Now,
            };

            string initialPassword = "admin123";

            var result = await userManager.CreateAsync(user, initialPassword);
            if (result.Succeeded)
            {
                // set the user role
                await userManager.AddToRoleAsync(user, "admin");
                Console.WriteLine("Admin user created successfully! Please update the initial password!");
                Console.WriteLine("Email: " + user.Email);
                Console.WriteLine("Initial password: " + initialPassword);
            }
        }
    }
}
