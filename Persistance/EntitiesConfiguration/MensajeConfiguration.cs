using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealStateApp.Core.Domain.Entities;
namespace RealStateApp.Persistance.EntitiesConfiguration
{

    public class MensajeConfiguration : IEntityTypeConfiguration<Mensaje>
    {
        public void Configure(EntityTypeBuilder<Mensaje> builder)
        {
            builder.Property(m => m.Contenido).IsRequired();

            builder.HasOne(m => m.Cliente)
                .WithMany(u => u.MensajesComoCliente)
                .HasForeignKey(m => m.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(m => m.Agente)
                .WithMany(u => u.MensajesComoAgente)
                .HasForeignKey(m => m.AgenteId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(m => m.Propiedad)
                .WithMany(p => p.Mensajes)
                .HasForeignKey(m => m.PropiedadId)
                .OnDelete(DeleteBehavior.Restrict);
            // Restrict también aquí: si permites cascada desde Propiedad Y desde
            // ApplicationUser hacia la misma tabla Mensaje, SQL Server tira el
            // error de "multiple cascade paths". Restrict en Propiedad rompe ese ciclo.
        }
    }
}