using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using RealStateApp.Core.Domain.Entities;

namespace RealStateApp.Persistance.Seeders;

public static class IdentitySeeder
{
    public static async Task SeedRolesAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var roles = new[] { "Administrador", "Desarrollador", "Cliente", "Agente" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    public static async Task SeedUsersAsync(IServiceProvider services)
    {
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        var usuarios = new[]
        {
            new { Email = "admin@realstateapp.com", Nombre = "Admin", Apellido = "Sistema", Telefono = "8090000001", Cedula = "001-0000001-1", Rol = "Administrador" },
            new { Email = "dev@realstateapp.com", Nombre = "Dev", Apellido = "Sistema", Telefono = "8090000002", Cedula = "001-0000002-2", Rol = "Desarrollador" },
            new { Email = "cliente@realstateapp.com", Nombre = "Cliente", Apellido = "Demo", Telefono = "8090000003", Cedula = "001-0000003-3", Rol = "Cliente" },
            new { Email = "agente@realstateapp.com", Nombre = "Agente", Apellido = "Demo", Telefono = "8090000004", Cedula = "001-0000004-4", Rol = "Agente" },
        };

        foreach (var u in usuarios)
        {
            if (await userManager.FindByEmailAsync(u.Email) is not null)
                continue;

            var user = new ApplicationUser
            {
                UserName = u.Email,
                Email = u.Email,
                EmailConfirmed = true,
                Nombre = u.Nombre,
                Apellido = u.Apellido,
                Telefono = u.Telefono,
                Cedula = u.Cedula,
                FotoPerfilUrl = "/images/default-user.png",
                EstaActivo = true,
                FechaRegistro = DateTime.Now
            };

            var result = await userManager.CreateAsync(user, "Password123!");

            if (result.Succeeded)
                await userManager.AddToRoleAsync(user, u.Rol);
        }
    }
}