using RealEstateApp.Core.Domain.Enums;
using RealStateApp.Core.Domain.Common;
using System;

namespace RealStateApp.Core.Domain.Entities
{
    public class Oferta : BaseEntities
    {
        public string CompradorId { get; set; } = null!;
        public virtual ApplicationUser Comprador { get; set; } = null!;

        public int PropiedadId { get; set; }
        public virtual Propiedad Propiedad { get; set; } = null!;

        public decimal Monto { get; set; }
        public EstadoOferta Estado { get; set; }
        public DateTime FechaOferta { get; set; }
    }
}