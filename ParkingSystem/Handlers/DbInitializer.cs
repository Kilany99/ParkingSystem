using Microsoft.AspNetCore.Identity;

namespace ParkingSystem.Handlers
{
    public class DbInitializer
    {
        public static async Task InitializeAsync(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            string[] roleNames = { "Operator", "User" };
            IdentityResult roleResult;

            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    // Create the roles
                    roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Create a default admin user
            IdentityUser user = await userManager.FindByEmailAsync("asdghalb@hotmail.com");

            if (user == null)
            {
                user = new IdentityUser()
                {
                    UserName = "asdghalb",
                    Email = "admin@System.com",
                };
                await userManager.CreateAsync(user, "Admin@123");
            }

            // Assign the admin role to the admin user
            if (!await userManager.IsInRoleAsync(user, "Operator"))
            {
                await userManager.AddToRoleAsync(user, "Operator");
            }
        }
    }
}
