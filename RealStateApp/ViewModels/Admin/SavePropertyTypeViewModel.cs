using System.ComponentModel.DataAnnotations;

namespace RealStateApp.Core.Application.ViewModels.Admin
{
    public class SavePropertyTypeViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Debe completar todos los campos requeridos.")]
        [Display(Name = "Nombre del tipo de propiedad")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "Debe completar todos los campos requeridos.")]
        [Display(Name = "Descripción")]
        public string Descripcion { get; set; }
    }
}