using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealStateApp.Core.Domain.Entities;

namespace RealStateApp.Persistance.EntitiesConfiguration

{
    public class TipoPropiedadConfiguration : IEntityTypeConfiguration<TipoPropiedad>
    {
        public void Configure(EntityTypeBuilder<TipoPropiedad> builder)
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
