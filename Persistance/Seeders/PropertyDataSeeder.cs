using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RealStateApp.Core.Domain.Entities;
using RealStateApp.Persistance.Contexts;

namespace RealStateApp.Persistance.Seeders;

public static class PropertyDataSeeder
{
    public static async Task SeedDefaultDataAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // 1. Sembrar TipoPropiedad (Verificando uno por uno)
        var tiposPropiedad = new[]
        {
            new TipoPropiedad { Nombre = "Casa", Descripcion = "Casa residencial unifamiliar" },
            new TipoPropiedad { Nombre = "Apartamento", Descripcion = "Apartamento en edificio o condominio" },
            new TipoPropiedad { Nombre = "Solar", Descripcion = "Terreno o solar para construcción" }
        };

        foreach (var item in tiposPropiedad)
        {
            if (!await context.TipoPropiedades.AnyAsync(tp => tp.Nombre == item.Nombre))
            {
                await context.TipoPropiedades.AddAsync(item);
            }
        }

        // 2. Sembrar TipoVenta (Verificando uno por uno)
        var tiposVenta = new[]
        {
            new TipoVenta { Nombre = "Venta", Descripcion = "Propiedad disponible para compra" },
            new TipoVenta { Nombre = "Alquiler", Descripcion = "Propiedad disponible para renta mensual" }
        };

        foreach (var item in tiposVenta)
        {
            if (!await context.TipoVentas.AnyAsync(tv => tv.Nombre == item.Nombre))
            {
                await context.TipoVentas.AddAsync(item);
            }
        }

        // 3. Sembrar Mejoras (Verificando uno por uno)
        var mejoras = new[]
        {
            new Mejora { Nombre = "Piscina", Descripcion = "Área de piscina privada" },
            new Mejora { Nombre = "Patio", Descripcion = "Patio o jardín trasero" },
            new Mejora { Nombre = "Seguridad 24/7", Descripcion = "Caseta de vigilancia y control de acceso" }
        };

        foreach (var item in mejoras)
        {
            if (!await context.Mejoras.AnyAsync(m => m.Nombre == item.Nombre))
            {
                await context.Mejoras.AddAsync(item);
            }
        }

        await context.SaveChangesAsync();
    }
}