using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealStateApp.Core.Domain.Entities;

namespace RealStateApp.Persistance.EntitiesConfiguration
{

    public class TipoVentaConfiguration : IEntityTypeConfiguration<TipoVenta>
    {
        public void Configure(EntityTypeBuilder<TipoVenta> builder)
        {
            builder.Property(t => t.Nombre)
                .IsRequired()
                .HasMaxLength(100);

            builder.HasIndex(t => t.Nombre)
                .IsUnique();

            builder.Property(t => t.Descripcion)
                .IsRequired()
                .HasMaxLength(500);
        }
    }
}
