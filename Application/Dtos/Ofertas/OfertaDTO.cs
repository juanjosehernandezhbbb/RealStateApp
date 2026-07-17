using System;

namespace RealStateApp.Core.Application.DTOs.Ofertas
{
    public class OfertaDTO
    {
        public int Id { get; set; }
        public int PropiedadId { get; set; }
        public string CompradorId { get; set; } = null!;
        public string CompradorNombre { get; set; } = null!;
        public decimal Monto { get; set; }
        public string Estado { get; set; } = null!; // "Pendiente", "Rechazada", "Aceptada"
        public DateTime FechaOferta { get; set; }
    }
}