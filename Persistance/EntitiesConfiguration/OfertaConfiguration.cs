using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealStateApp.Core.Domain.Entities;
namespace RealStateApp.Persistance.EntitiesConfiguration
{

    public class OfertaConfiguration : IEntityTypeConfiguration<Oferta>
    {
        public void Configure(EntityTypeBuilder<Oferta> builder)
        {
            builder.Property(o => o.Monto).HasColumnType("decimal(18,2)");

            builder.HasOne(o => o.Comprador)
                .WithMany(u => u.Ofertas)
                .HasForeignKey(o => o.CompradorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(o => o.Propiedad)
                .WithMany(p => p.Ofertas)
                .HasForeignKey(o => o.PropiedadId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}