using AutoMapper;
using RealStateApp.Core.Application.DTOs.TipoVentas;
using RealStateApp.Core.Application.Interfaces.Repositories;
using RealStateApp.Core.Application.Interfaces.Services;
using RealStateApp.Core.Domain.Entities;

namespace RealStateApp.Core.Application.Services
{
    public class TipoVentaService : GenericService<SaveTipoVentaDTO, TipoVentaDTO, TipoVenta>, ITipoVentaService
    {
        public TipoVentaService(IGenericRepository<TipoVenta> repository, IMapper mapper) : base(repository, mapper)
        {
        }
    }
}