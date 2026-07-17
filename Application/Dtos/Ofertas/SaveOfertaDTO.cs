using System;

namespace RealStateApp.Core.Application.DTOs.Ofertas
{
    public class SaveOfertaDTO
    {
        public int Id { get; set; }
        public int PropiedadId { get; set; }
        public string CompradorId { get; set; } = null!;
        public decimal Monto { get; set; }
    }
}