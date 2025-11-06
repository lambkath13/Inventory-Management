using InventoryApp.Models;
using Microsoft.AspNetCore.Identity;

namespace InventoryApp.Data;

public static class Seed
{
    public static async Task EnsureRolesAndAdminAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var sp = scope.ServiceProvider;

        var roleManager = sp.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = sp.GetRequiredService<UserManager<AppUser>>();

        const string adminRole = "Admin";
        if (!await roleManager.RoleExistsAsync(adminRole))
        {
            await roleManager.CreateAsync(new IdentityRole(adminRole));
        }
        const string adminEmail = "admin@example.com";
        const string adminPassword = "Admin123!"; 

        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new AppUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                DisplayName = "Administrator",
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };

            var createRes = await userManager.CreateAsync(adminUser, adminPassword);
            if (!createRes.Succeeded)
            {
                var errors = string.Join(", ", createRes.Errors.Select(e => e.Description));
                throw new Exception($"Ошибка создания дефолтного админа: {errors}");
            }
        }

        if (!await userManager.IsInRoleAsync(adminUser, adminRole))
        {
            await userManager.AddToRoleAsync(adminUser, adminRole);
        }
    }
}
