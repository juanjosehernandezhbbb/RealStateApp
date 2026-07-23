using System;
using System.Collections.Generic;

namespace RealStateApp.Core.Application.DTOs.Propiedades
{
    public class PropiedadDTO
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = null!;
        public int TipoPropiedadId { get; set; }
        public string TipoPropiedadNombre { get; set; } = null!;
        public int TipoVentaId { get; set; }
        public string TipoVentaNombre { get; set; } = null!;
        public string AgenteId { get; set; } = null!;
        public string AgenteNombre { get; set; } = null!;
        public string AgenteTelefono { get; set; } = null!;
        public string AgenteEmail { get; set; } = null!;
        public string AgenteFotoPerfilUrl { get; set; } = null!;
        public decimal Precio { get; set; }
        public string Descripcion { get; set; } = null!;
        public decimal Tamano { get; set; }
        public int CantidadHabitaciones { get; set; }
        public int CantidadBanos { get; set; }
        public string Estado { get; set; } = null!;
        public DateTime FechaCreacion { get; set; }

        public List<string> ImagenesUrls { get; set; } = new();
        public List<string> Mejoras { get; set; } = new();
        public List<int> MejorasIds { get; set; } = new List<int>();
        public bool EsFavorito { get; set; }
    }
}