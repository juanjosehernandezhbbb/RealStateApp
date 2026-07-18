using RealStateApp.Core.Domain.Entities;

namespace RealStateApp.Core.Application.Interfaces.Repositories
{
    public interface ITipoVentaRepository : IGenericRepository<TipoVenta>
    {
        Task<bool> ExisteNombreAsync(string nombre, int? idToIgnore = null);

    }
}