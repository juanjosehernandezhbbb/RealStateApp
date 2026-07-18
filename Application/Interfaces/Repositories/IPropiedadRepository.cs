using RealStateApp.Core.Domain.Entities;
using System.Threading.Tasks;

namespace RealStateApp.Core.Application.Interfaces.Repositories
{
    public interface IPropiedadRepository : IGenericRepository<Propiedad>
    {
        Task<bool> ExisteCodigoAsync(string codigo);
        Task DeleteConDependenciasAsync(int propiedadId); 
    }
}