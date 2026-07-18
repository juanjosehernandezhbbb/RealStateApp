using Microsoft.EntityFrameworkCore;
using RealStateApp.Core.Application.Interfaces.Repositories;
using RealStateApp.Core.Domain.Entities;
using RealStateApp.Persistance.Contexts;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RealStateApp.Persistance.Repositories
{
    public class FavoritoRepository : GenericRepository<Favorito>, IFavoritoRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public FavoritoRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public override async Task<List<Favorito>> GetAllAsync()
        {
            return await _dbContext.Set<Favorito>()
                .Include(f => f.Favorita).ThenInclude(p => p.TipoPropiedad)
                .Include(f => f.Favorita).ThenInclude(p => p.TipoVenta)
                .Include(f => f.Favorita).ThenInclude(p => p.Agente)
                .Include(f => f.Favorita).ThenInclude(p => p.Imagenes)
                .Include(f => f.Favorita).ThenInclude(p => p.PropiedadMejoras).ThenInclude(pm => pm.Mejora)
                .ToListAsync();
        }

        public override async Task<Favorito> GetByIdAsync(int id)
        {
            return await _dbContext.Set<Favorito>()
                .Include(f => f.Favorita).ThenInclude(p => p.TipoPropiedad)
                .Include(f => f.Favorita).ThenInclude(p => p.TipoVenta)
                .Include(f => f.Favorita).ThenInclude(p => p.Agente)
                .Include(f => f.Favorita).ThenInclude(p => p.Imagenes)
                .Include(f => f.Favorita).ThenInclude(p => p.PropiedadMejoras).ThenInclude(pm => pm.Mejora)
                .FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task<bool> ExisteFavoritoAsync(string usuarioId, int propiedadId)
        {
            return await _dbContext.Set<Favorito>().AnyAsync(f => f.UsuarioId == usuarioId && f.PropiedadFavoritaId == propiedadId);
        }
    }
}