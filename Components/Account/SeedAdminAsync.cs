using BoardHoard.Data;              // <-- to see ApplicationUser
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

public static class AdminSeeder
{
    public static async Task SeedAdminAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>(); // <-- changed

        const string adminRole = "Admin";
        const string adminEmail = "zackruggles177@gmail.com";

        if (!await roleManager.RoleExistsAsync(adminRole))
            await roleManager.CreateAsync(new IdentityRole(adminRole));

        var user = await userManager.FindByEmailAsync(adminEmail);
        if (user != null && !await userManager.IsInRoleAsync(user, adminRole))
        {
            var result = await userManager.AddToRoleAsync(user, adminRole);
            if (!result.Succeeded)
                throw new Exception("Failed to add user to Admin role: " +
                    string.Join("; ", result.Errors.Select(e => e.Description)));
        }
    }
}
