using RealStateApp.Core.Application.DTOs.Favoritos;
using RealStateApp.Core.Domain.Entities;
using System.Threading.Tasks;

namespace RealStateApp.Core.Application.Interfaces.Services
{
    public interface IFavoritoService : IGenericService<SaveFavoritoDTO, FavoritoDTO, Favorito>
    {
        new Task<SaveFavoritoDTO?> AddAsync(SaveFavoritoDTO dto);
    }
}