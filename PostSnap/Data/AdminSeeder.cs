using Microsoft.AspNetCore.Identity;

namespace PostSnap.Data
{
    public static class AdminSeeder
    {
        public static async Task SeedAdmin(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            var adminEmail = "admin@example.com";
            var adminPassword = "AdminPassword123!";

            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                var newAdmin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(newAdmin, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(newAdmin, "Admin");
                }
            }
        }
    }
}
