using RealStateApp.Core.Domain.Entities;
using System.Threading.Tasks;

namespace RealStateApp.Core.Application.Interfaces.Repositories
{
    public interface IFavoritoRepository : IGenericRepository<Favorito>
    {
        Task<bool> ExisteFavoritoAsync(string usuarioId, int propiedadId);
    }
}