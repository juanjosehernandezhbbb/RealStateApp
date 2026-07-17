using System;

namespace RealStateApp.Core.Application.DTOs.Mensajes
{
    public class MensajeDTO
    {
        public int Id { get; set; }
        public int PropiedadId { get; set; }
        public string ClienteId { get; set; } = null!;
        public string ClienteNombre { get; set; } = null!;
        public string AgenteId { get; set; } = null!;
        public string AgenteNombre { get; set; } = null!;
        public string EmisorId { get; set; } = null!; 
        public string Contenido { get; set; } = null!;
        public DateTime FechaEnvio { get; set; }
    }
}