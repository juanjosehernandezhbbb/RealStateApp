using Microsoft.EntityFrameworkCore;
using RealStateApp.Core.Application.Interfaces.Repositories;
using RealStateApp.Core.Domain.Entities;
using RealStateApp.Persistance.Contexts;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RealStateApp.Persistance.Repositories
{
    public class TipoPropiedadRepository : GenericRepository<TipoPropiedad>, ITipoPropiedadRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public TipoPropiedadRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public override async Task<List<TipoPropiedad>> GetAllAsync()
        {
            return await _dbContext.Set<TipoPropiedad>()
                .Include(t => t.Propiedades)
                .ToListAsync();
        }

        public override async Task<TipoPropiedad> GetByIdAsync(int id)
        {
            return await _dbContext.Set<TipoPropiedad>()
                .Include(t => t.Propiedades)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<bool> ExisteNombreAsync(string nombre, int? idToIgnore = null)
        {
            return await _dbContext.Set<TipoPropiedad>().AnyAsync(x => x.Nombre == nombre && x.Id != idToIgnore); 
        }
    }
}