using RealStateApp.Core.Domain.Enums;
using RealStateApp.Core.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealStateApp.Core.Domain.Entities
{
    public class Propiedad: BaseEntities
    {
        
            public string Codigo { get; set; }
            public int TipoPropiedadId { get; set; }
            public int TipoVentaId { get; set; }
            public string AgenteId { get; set; }

            public decimal Precio { get; set; }
            public string Descripcion { get; set; }
            public decimal Tamano { get; set; }

            public int CantidadHabitaciones { get; set; }
            public int CantidadBanos { get; set; }

            public EstadoPropiedad Estado { get; set; }
            public DateTime FechaCreacion { get; set; }

            // Navegaciones N:1
            public TipoPropiedad TipoPropiedad { get; set; }
            public TipoVenta TipoVenta { get; set; }
            public ApplicationUser Agente { get; set; }

            // Navegaciones 1:N
            public ICollection<PropiedadImagen> Imagenes { get; set; } = null!;
        public ICollection<PropiedadMejora> PropiedadMejoras { get; set; } = null!;
            public ICollection<Favorito> Favoritos { get; set; } = null!;
        public ICollection<Oferta> Ofertas { get; set; } = null!;
        public ICollection<Mensaje> Mensajes { get; set; } = null!;
    }




    
}
