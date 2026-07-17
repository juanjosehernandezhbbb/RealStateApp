using RealStateApp.Core.Application.DTOs.TipoPropiedades;
using RealStateApp.Core.Domain.Entities;

namespace RealStateApp.Core.Application.Interfaces.Services
{
    public interface ITipoPropiedadService : IGenericService<SaveTipoPropiedadDTO, TipoPropiedadDTO, TipoPropiedad>
    {
    }
}
