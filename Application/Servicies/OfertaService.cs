using AutoMapper;
using RealStateApp.Core.Application.DTOs.Ofertas;
using RealStateApp.Core.Application.Interfaces.Repositories;
using RealStateApp.Core.Application.Interfaces.Services;
using RealStateApp.Core.Domain.Entities;

namespace RealStateApp.Core.Application.Services
{
    public class OfertaService : GenericService<SaveOfertaDTO, OfertaDTO, Oferta>, IOfertaService
    {
        public OfertaService(IGenericRepository<Oferta> repository, IMapper mapper) : base(repository, mapper)
        {
        }
    }
}