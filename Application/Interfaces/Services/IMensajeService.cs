using RealStateApp.Core.Application.DTOs.Mensajes;
using RealStateApp.Core.Domain.Entities;

namespace RealStateApp.Core.Application.Interfaces.Services
{
    public interface IMensajeService : IGenericService<SaveMensajeDTO, MensajeDTO, Mensaje>
    {
    }
}