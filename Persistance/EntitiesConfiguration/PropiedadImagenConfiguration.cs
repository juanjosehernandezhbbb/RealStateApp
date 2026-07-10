using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealStateApp.Core.Domain.Entities;
namespace RealStateApp.Persistance.EntitiesConfiguration
{

    public class PropiedadImagenConfiguration : IEntityTypeConfiguration<PropiedadImagen>
    {
        public void Configure(EntityTypeBuilder<PropiedadImagen> builder)
        {
            builder.Property(pi => pi.ImagenUrl).IsRequired();

            builder.HasOne(pi => pi.Propiedad)
                .WithMany(p => p.Imagenes)
                .HasForeignKey(pi => pi.PropiedadId)
                .OnDelete(DeleteBehavior.Cascade); // al borrar la propiedad, se borran sus imágenes
        }
    }
}