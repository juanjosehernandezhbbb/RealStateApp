using RealStateApp.Core.Application.DTOs.Propiedades;
using RealStateApp.Core.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RealStateApp.Core.Application.Interfaces.Services
{
    public interface IPropiedadService : IGenericService<SavePropiedadDTO, PropiedadDTO, Propiedad>
    {
        Task UpdateAsync(SavePropiedadDTO dto, int id, string agenteId);
        Task DeleteAsync(int id, string agenteId);

        Task<PropiedadDTO> GetByCodeAsync(string codigo);
        Task<List<PropiedadDTO>> GetPropiedadesByAgenteAsync(string agenteId);

        Task<List<PropiedadDTO>> GetAllAsync();
    }
}