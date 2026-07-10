using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RealStateApp.Core.Domain.Common;

namespace RealStateApp.Core.Domain.Entities
{
    public class TipoPropiedad: BaseEntities
    {
        public required string Nombre { get; set; }
        public required string Descripcion { get; set; }

        public ICollection<Propiedad> Propiedades { get; set; } = new List<Propiedad>();
    }
}
