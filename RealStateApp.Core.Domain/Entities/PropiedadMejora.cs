    
using RealStateApp.Core.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealStateApp.Core.Domain.Entities
{
    public class PropiedadMejora 
    {
        public int PropiedadId { get; set; }
        public Propiedad Propiedad { get; set; } = null!;

        public int MejoraId { get; set; }
        public Mejora Mejora { get; set; }
    }
}
