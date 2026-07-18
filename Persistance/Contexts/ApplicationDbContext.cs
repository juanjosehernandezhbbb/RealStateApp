using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer;
using RealStateApp.Core.Domain.Entities;

namespace RealStateApp.Persistance.Contexts
{
 
        public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
        {
            public DbSet<TipoPropiedad> TipoPropiedades { get; set; } = null!;
            public DbSet<TipoVenta> TipoVentas { get; set; } = null!;
            public DbSet<Mejora> Mejoras { get; set; } = null!;
            public DbSet<Propiedad> Propiedades { get; set; } = null!;
            public DbSet<PropiedadImagen> PropiedadImagenes { get; set; } = null!;
            public DbSet<PropiedadMejora> PropiedadMejoras { get; set; } = null!;
            public DbSet<Favorito> Favoritos { get; set; } = null!;
            public DbSet<Oferta> Ofertas { get; set; } = null!;
            public DbSet<Mensaje> Mensajes { get; set; } = null!;

       
        
          protected override void OnModelCreating(ModelBuilder builder)
          {
            base.OnModelCreating(builder);

            builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
          }

        }
}