using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealStateApp.Core.Domain.Entities;

namespace RealStateApp.Persistance.EntitiesConfiguration;

public class FavoritoConfiguration : IEntityTypeConfiguration<Favorito>
{
    public void Configure(EntityTypeBuilder<Favorito> builder)
    {
        builder.HasIndex(f => new { f.UsuarioId, f.PropiedadFavoritaId }).IsUnique();

        builder.HasOne(f => f.Usuario)
            .WithMany(u => u.Favoritos)
            .HasForeignKey(f => f.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(f => f.Favorita)
            .WithMany(p => p.Favoritos)
            .HasForeignKey(f => f.PropiedadFavoritaId)
            .OnDelete(DeleteBehavior.Cascade); 
    }
}
