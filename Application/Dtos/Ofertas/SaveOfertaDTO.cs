using System;
using System.Text.Json.Serialization;

namespace RealStateApp.Core.Application.DTOs.Ofertas
{
    public class SaveOfertaDTO
    {
        public int Id { get; set; }
        public int PropiedadId { get; set; }

        [JsonIgnore]
        public string CompradorId { get; set; } = null!;
        public decimal Monto { get; set; }
    }
}