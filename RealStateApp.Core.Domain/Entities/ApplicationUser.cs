using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealStateApp.Core.Domain.Entities
{
    public class ApplicationUser: IdentityUser
    {

        public string Nombre { get; set; } = null!;
        public string Apellido { get; set; } = null!;
        public string Telefono { get; set; } = null!;
        public string? FotoPerfilUrl { get; set; }
        public string? Cedula { get; set; }
        public DateTime FechaRegistro { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public bool EstaActivo { get; set; }        
        public virtual ICollection<Propiedad> Propiedades { get; set; } = new List<Propiedad>();
        public virtual ICollection<Favorito> Favoritos { get; set; } = new List<Favorito>();
        public virtual ICollection<Oferta> Ofertas { get; set; }= new List<Oferta>();
        public virtual ICollection<Mensaje> MensajesComoCliente { get; set; } = new List<Mensaje>();
        public virtual ICollection<Mensaje> MensajesComoAgente { get; set; } = new List<Mensaje>();

    }
}
