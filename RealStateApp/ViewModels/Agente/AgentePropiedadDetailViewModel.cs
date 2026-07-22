using RealStateApp.Core.Application.DTOs.Propiedades;
using RealStateApp.Core.Application.DTOs.Ofertas; 
using RealStateApp.Core.Application.DTOs.Mensajes; 
using System.Collections.Generic;

namespace RealStateApp.WebApp.ViewModels.Agente
{
    public class AgentePropiedadDetailViewModel
    {
        public PropiedadDTO Propiedad { get; set; } = null!;
        public List<OfertaDTO> Ofertas { get; set; } = new();
        public List<MensajeDTO> Mensajes { get; set; } = new();
    }
}