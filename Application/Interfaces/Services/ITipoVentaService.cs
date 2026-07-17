using RealStateApp.Core.Application.DTOs.TipoVentas;
using RealStateApp.Core.Domain.Entities;

namespace RealStateApp.Core.Application.Interfaces.Services
{
    public interface ITipoVentaService : IGenericService<SaveTipoVentaDTO, TipoVentaDTO, TipoVenta>
    {
    }
}