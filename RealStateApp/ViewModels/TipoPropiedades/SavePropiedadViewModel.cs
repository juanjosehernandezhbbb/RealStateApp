using Microsoft.AspNetCore.Http;
using RealStateApp.Core.Application.DTOs.Mejoras;
using RealStateApp.Core.Application.DTOs.TipoPropiedades;
using RealStateApp.Core.Application.DTOs.TipoVentas;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RealStateApp.WebApp.ViewModels.Propiedades
{
    public class SavePropiedadViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un tipo de propiedad")]
        [Display(Name = "Tipo de Propiedad")]
        public int TipoPropiedadId { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un tipo de venta")]
        [Display(Name = "Tipo de Venta")]
        public int TipoVentaId { get; set; }

        [Required(ErrorMessage = "El precio es obligatorio")]
        [DataType(DataType.Currency)]
        public decimal Precio { get; set; }

        [Required(ErrorMessage = "La descripción es obligatoria")]
        [Display(Name = "Descripción")]
        public string Descripcion { get; set; } = null!;

        [Required(ErrorMessage = "El tamaño es obligatorio")]
        [Display(Name = "Tamaño (mts2)")]
        public decimal Tamano { get; set; }

        [Required(ErrorMessage = "La cantidad de habitaciones es obligatoria")]
        [Display(Name = "Habitaciones")]
        public int CantidadHabitaciones { get; set; }

        [Required(ErrorMessage = "La cantidad de baños es obligatoria")]
        [Display(Name = "Baños")]
        public int CantidadBanos { get; set; }

        public string? AgenteId { get; set; }

        // Múltiples archivos para las fotos de la propiedad
        [Display(Name = "Imágenes de la propiedad (Máximo 4)")]
        public List<IFormFile>? ArchivosImagenes { get; set; }

        [Display(Name = "Mejoras (Opcional)")]
        public List<int> MejorasIds { get; set; } = new();

        // --- Colecciones para renderizar los Dropdowns y Checkboxes en la Vista ---
        public List<TipoPropiedadDTO>? TiposPropiedades { get; set; }
        public List<TipoVentaDTO>? TiposVentas { get; set; }
        public List<MejoraDTO>? Mejoras { get; set; }
    }
}