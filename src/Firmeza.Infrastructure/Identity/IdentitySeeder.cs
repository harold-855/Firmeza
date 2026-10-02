using Firmeza.Domain.Constants;
using Microsoft.AspNetCore.Identity;

namespace Firmeza.Infrastructure.Identity;

public static class IdentitySeeder
{
    public static async Task SeedRolesAndAdminAsync(
        RoleManager<IdentityRole> roleManager,
        UserManager<IdentityUser> userManager)
    {
        string[] roles =
        [
            Roles.Administrador,
            Roles.Cliente
        ];

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // Crear usuario Administrador por defecto si no existe
        const string adminEmail = "admin@firmeza.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            var admin = new IdentityUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(admin, "Admin123*");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, Roles.Administrador);
            }
        }

        // Crear usuario Cliente por defecto (para pruebas del bloqueo en Razor) si no existe
        const string clientEmail = "cliente@firmeza.com";
        var clientUser = await userManager.FindByEmailAsync(clientEmail);
        if (clientUser == null)
        {
            var client = new IdentityUser
            {
                UserName = clientEmail,
                Email = clientEmail,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(client, "Cliente123*");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(client, Roles.Cliente);
            }
        }
    }
}