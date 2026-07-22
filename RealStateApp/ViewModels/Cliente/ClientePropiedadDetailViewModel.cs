using RealStateApp.Core.Application.DTOs.Mensajes;
using RealStateApp.Core.Application.DTOs.Ofertas;
using RealStateApp.Core.Application.DTOs.Propiedades;
namespace RealStateApp.Core.Application.ViewModels.Cliente
{
    public class ClientePropiedadDetailViewModel
    {
        public PropiedadDTO Propiedad { get; set; } = null!;

        public List<OfertaDTO> Ofertas { get; set; } = new();

        public List<MensajeDTO> Mensajes { get; set; } = new();

        public string NuevoMensaje { get; set; } = null!;

        public decimal MontoOferta { get; set; }

        public bool TieneOfertaPendiente { get; set; }
        public bool TieneOfertaAceptadaGlobal { get; set; }
    }
}