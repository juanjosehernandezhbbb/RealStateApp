using RealStateApp.Core.Application.DTOs.Mensajes;
using RealStateApp.Core.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RealStateApp.Core.Application.Interfaces.Services
{
    public interface IMensajeService : IGenericService<SaveMensajeDTO, MensajeDTO, Mensaje>
    {
        Task<List<MensajeDTO>> GetMensajesPorAgenteAsync(string agenteId);
        Task<List<MensajeDTO>> GetMensajesPorPropiedadAsync(int propiedadId, string clienteId);
        Task<List<MensajeDTO>> GetMensajesPorPropiedadAsync(int propiedadId); // <-- Nuevo método para el agente
    }
}