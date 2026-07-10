using RealStateApp.Core.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealStateApp.Core.Domain.Entities
{
    public class Mensaje: BaseEntities
    {
        public string ClienteId { get; set; } = null!;
        public virtual ApplicationUser Cliente { get; set; } = null!;

        public string AgenteId { get; set; } = null!;
        public virtual ApplicationUser Agente { get; set; } = null!;         
        public int PropiedadId { get; set; }
        public Propiedad Propiedad { get; set; }

        public string Contenido { get; set; }
        public string EmisorId { get; set; }
        public DateTime FechaEnvio { get; set; }

    }
    
}
