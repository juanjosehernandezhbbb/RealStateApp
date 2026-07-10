using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealStateApp.Core.Domain.Entities;

namespace RealStateApp.Persistance.EntitiesConfiguration

{
    public class MejoraConfiguration : IEntityTypeConfiguration<Mejora>
    {
        public void Configure(EntityTypeBuilder<Mejora> builder)
        {
            builder.Property(m => m.Nombre)
                .IsRequired()
                .HasMaxLength(100);

            builder.HasIndex(m => m.Nombre)
                .IsUnique();

            builder.Property(m => m.Descripcion)
                .IsRequired()
                .HasMaxLength(500);
        }
    }
}