using RealStateApp.Core.Domain.Entities;

namespace RealStateApp.Core.Application.Interfaces.Repositories
{
    public interface ITipoPropiedadRepository : IGenericRepository<TipoPropiedad>
    {
        Task<bool> ExisteNombreAsync(string nombre, int? idToIgnore = null);

    }
}