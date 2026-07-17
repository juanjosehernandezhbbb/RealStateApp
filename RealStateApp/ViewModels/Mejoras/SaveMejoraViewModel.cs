using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;

namespace RealStateApp.ViewModels.Mejoras
{
    public class SaveMejoraViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre de la mejora es requerido.")]
        public string Nombre { get; set; } = null!;

        [Required(ErrorMessage = "La descripción de la mejora es requerida.")]
        public string Descripcion { get; set; } = null!;
    }
}