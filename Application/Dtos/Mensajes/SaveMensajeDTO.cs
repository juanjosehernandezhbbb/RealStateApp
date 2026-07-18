using System.Text.Json.Serialization;

namespace RealStateApp.Core.Application.DTOs.Mensajes
{
    public class SaveMensajeDTO
    {
        public int Id { get; set; }
        public int PropiedadId { get; set; }

        [JsonIgnore]
        public string ClienteId { get; set; } = null!;
        [JsonIgnore]
        public string AgenteId { get; set; } = null!;
        [JsonIgnore]
        public string EmisorId { get; set; } = null!;
        public string Contenido { get; set; } = null!;
    }
}