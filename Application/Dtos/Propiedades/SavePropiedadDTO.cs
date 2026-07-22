using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RealStateApp.Core.Application.DTOs.Propiedades
{
    public class SavePropiedadDTO
    {
        public int Id { get; set; }
        public int TipoPropiedadId { get; set; }
        public int TipoVentaId { get; set; }
        public decimal Precio { get; set; }
        public string Descripcion { get; set; } = null!;
        public decimal Tamano { get; set; }
        public int CantidadHabitaciones { get; set; }
        public int CantidadBanos { get; set; }

        [JsonIgnore]
        public string AgenteId { get; set; } = null!;

        public List<string> ImagenesUrls { get; set; } = new();
        public List<int> MejorasIds { get; set; } = new();

        // Propiedad necesaria para recibir los archivos en el servicio/repositorio
        public List<IFormFile>? ArchivosImagenes { get; set; }
    }
}