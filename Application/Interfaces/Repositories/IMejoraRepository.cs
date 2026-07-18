using RealStateApp.Core.Domain.Entities;

namespace RealStateApp.Core.Application.Interfaces.Repositories
{
    public interface IMejoraRepository : IGenericRepository<Mejora>
    {
        Task<bool> ExisteNombreAsync(string nombre, int? idToIgnore = null);

    }
}