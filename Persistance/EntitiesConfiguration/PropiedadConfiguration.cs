using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealStateApp.Core.Domain.Entities;
namespace RealStateApp.Persistance.EntitiesConfiguration
{

    public class PropiedadConfiguration : IEntityTypeConfiguration<Propiedad>
    {
        public void Configure(EntityTypeBuilder<Propiedad> builder)
        {
            builder.Property(p => p.Codigo).IsRequired().HasMaxLength(6);
            builder.HasIndex(p => p.Codigo).IsUnique();

            builder.Property(p => p.Precio).HasColumnType("decimal(18,2)");
            builder.Property(p => p.Tamano).HasColumnType("decimal(18,2)");

            builder.HasOne(p => p.TipoPropiedad)
                .WithMany(t => t.Propiedades)
                .HasForeignKey(p => p.TipoPropiedadId)
                .OnDelete(DeleteBehavior.Cascade); // al borrar el tipo, se borran sus propiedades

            builder.HasOne(p => p.TipoVenta)
                .WithMany(t => t.Propiedades)
                .HasForeignKey(p => p.TipoVentaId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(p => p.Agente)
                .WithMany(u => u.Propiedades)
                .HasForeignKey(p => p.AgenteId)
                .OnDelete(DeleteBehavior.Restrict); // no borrar propiedades si se borra el agente
        }
    }
}