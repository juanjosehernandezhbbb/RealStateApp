using RealStateApp.Core.Application.DTOs.Propiedades;
using RealStateApp.Core.Domain.Entities;

namespace RealStateApp.Core.Application.Interfaces.Services
{
    public interface IPropiedadService : IGenericService<SavePropiedadDTO, PropiedadDTO, Propiedad>
    {
    }
}
