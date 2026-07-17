using RealStateApp.Core.Application.DTOs.Favoritos;
using RealStateApp.Core.Domain.Entities;

namespace RealStateApp.Core.Application.Interfaces.Services
{
    public interface IFavoritoService : IGenericService<SaveFavoritoDTO, FavoritoDTO, Favorito>
    {
    }
}