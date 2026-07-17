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

            builder.Entity<ApplicationUser>().ToTable("Users");
            builder.Entity<Propiedad>().ToTable("Propiedades");

            // Configuración de llaves primarias compuestas
            builder.Entity<PropiedadMejora>()
                .HasKey(pm => new { pm.PropiedadId, pm.MejoraId });


            builder.Entity<Propiedad>()
                .HasOne(p => p.Agente)
                .WithMany(u => u.Propiedades)
                .HasForeignKey(p => p.AgenteId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Mensaje>()
                .HasOne(m => m.Cliente)
                .WithMany(u => u.MensajesComoCliente)
                .HasForeignKey(m => m.ClienteId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Mensaje>()
                .HasOne(m => m.Agente)
                .WithMany(u => u.MensajesComoAgente)
                .HasForeignKey(m => m.AgenteId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Oferta>()
                .HasOne(o => o.Comprador)
                .WithMany(u => u.Ofertas)
                .HasForeignKey(o => o.CompradorId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Favorito>()
                .HasOne(f => f.Usuario)
                .WithMany(u => u.Favoritos)
                .HasForeignKey(f => f.UsuarioId)
                .OnDelete(DeleteBehavior.NoAction);

        }
        
    }
}