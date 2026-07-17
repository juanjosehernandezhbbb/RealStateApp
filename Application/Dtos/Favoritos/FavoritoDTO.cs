using RealStateApp.Core.Application.DTOs.Propiedades;
using System;

namespace RealStateApp.Core.Application.DTOs.Favoritos
{
    public class FavoritoDTO
    {
        public int Id { get; set; }
        public string UsuarioId { get; set; } = null!;
        public int PropiedadFavoritaId { get; set; }
        public PropiedadDTO Propiedad { get; set; } = null!; 
        public DateTime FechaAgregado { get; set; }
    }
}