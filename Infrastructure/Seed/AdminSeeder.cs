using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Seed;

public static class AdminSeeder
{
    public static async Task SeedAsync(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
    {
        const string email = "admin@anyfood.com";
        const string password = "Admin1234!";
        const string role = "Admin";

        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));

        if (await userManager.FindByEmailAsync(email) is not null)
            return;

        var admin = new User
        {
            UserName = email,
            Email = email,
            Name = "Admin",
            EmailConfirmed = true,
        };

        var result = await userManager.CreateAsync(admin, password);
        if (result.Succeeded)
            await userManager.AddToRoleAsync(admin, role);
    }
}
