using RealStateApp.Core.Application.DTOs.Ofertas;
using RealStateApp.Core.Domain.Entities;

namespace RealStateApp.Core.Application.Interfaces.Services
{
    public interface IOfertaService : IGenericService<SaveOfertaDTO, OfertaDTO, Oferta>
    {

        Task AceptarOfertaAsync(int ofertaId, string agenteId);
        Task RechazarOfertaAsync(int ofertaId, string agenteId);

    }
}