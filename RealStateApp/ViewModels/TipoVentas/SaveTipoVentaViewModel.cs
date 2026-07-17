using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;

namespace RealStateApp.ViewModels.TipoVentas
{
    public class SaveTipoVentaViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del tipo de venta es requerido.")]
        public string Nombre { get; set; } = null!;

        [Required(ErrorMessage = "La descripción del tipo de venta es requerida.")]
        public string Descripcion { get; set; } = null!;
    }
}