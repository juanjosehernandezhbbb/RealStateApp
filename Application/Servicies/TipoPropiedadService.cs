using AutoMapper;
using RealStateApp.Core.Application.DTOs.TipoPropiedades;
using RealStateApp.Core.Application.Interfaces.Repositories;
using RealStateApp.Core.Application.Interfaces.Services;
using RealStateApp.Core.Domain.Entities;

namespace RealStateApp.Core.Application.Services
{
    public class TipoPropiedadService : GenericService<SaveTipoPropiedadDTO, TipoPropiedadDTO, TipoPropiedad>, ITipoPropiedadService
    {
        public TipoPropiedadService(IGenericRepository<TipoPropiedad> repository, IMapper mapper) : base(repository, mapper)
        {
        }
    }
}