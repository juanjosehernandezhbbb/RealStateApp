using RealStateApp.Core.Domain.Common;
using System;

namespace RealStateApp.Core.Domain.Entities
{
    public class Favorito : BaseEntities
    {
        public string UsuarioId { get; set; } = null!;
        public virtual ApplicationUser Usuario { get; set; } = null!;

        public int PropiedadFavoritaId { get; set; }
        public virtual Propiedad Favorita { get; set; } = null!;

        public DateTime FechaAgregado { get; set; }
    }
}