using System.Collections.Generic;
using System.Threading.Tasks;

namespace RealStateApp.Core.Application.Interfaces.Services
{
    public interface IGenericService<SaveDTO, DTO, Model>
        where SaveDTO : class
        where DTO : class
        where Model : class
    {
        Task<SaveDTO> AddAsync(SaveDTO dto);
        Task UpdateAsync(SaveDTO dto, int id);
        Task DeleteAsync(int id);
        Task<List<DTO>> GetAllDTOsAsync();
        Task<SaveDTO> GetByIdSaveDTOAsync(int id);
    }
}