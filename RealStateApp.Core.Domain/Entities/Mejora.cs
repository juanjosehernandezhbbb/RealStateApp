using RealStateApp.Core.Domain.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealStateApp.Core.Domain.Entities
{
    public class Mejora: BaseEntities
    {
        public required string Nombre { get; set; }
        public required string Descripcion { get; set; }

        public ICollection<PropiedadMejora> PropiedadMejoras { get; set; }
    }
}

