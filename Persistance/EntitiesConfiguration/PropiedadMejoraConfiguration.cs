using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealStateApp.Core.Domain.Entities;
namespace RealStateApp.Persistance.EntitiesConfiguration
{

    public class PropiedadMejoraConfiguration : IEntityTypeConfiguration<PropiedadMejora>
    {
        public void Configure(EntityTypeBuilder<PropiedadMejora> builder)
        {
            builder.HasKey(pm => new { pm.PropiedadId, pm.MejoraId });

            builder.HasOne(pm => pm.Propiedad)
                .WithMany(p => p.PropiedadMejoras)
                .HasForeignKey(pm => pm.PropiedadId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(pm => pm.Mejora)
                .WithMany(m => m.PropiedadMejoras)
                .HasForeignKey(pm => pm.MejoraId)
                .OnDelete(DeleteBehavior.Cascade); // al borrar la mejora, solo se borra la relación, no la propiedad
        }
    }
}