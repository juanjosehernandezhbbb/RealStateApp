using Microsoft.EntityFrameworkCore;
using RealStateApp.Core.Application.Interfaces.Repositories;
using RealStateApp.Core.Domain.Entities;
using RealStateApp.Persistance.Contexts;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RealStateApp.Persistance.Repositories
{
    public class TipoVentaRepository : GenericRepository<TipoVenta>, ITipoVentaRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public TipoVentaRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public override async Task<List<TipoVenta>> GetAllAsync()
        {
            return await _dbContext.Set<TipoVenta>()
                .Include(t => t.Propiedades)
                .ToListAsync();
        }

        public override async Task<TipoVenta> GetByIdAsync(int id)
        {
            return await _dbContext.Set<TipoVenta>()
                .Include(t => t.Propiedades)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<bool> ExisteNombreAsync(string nombre, int? idToIgnore = null)
        {
            return await _dbContext.Set<TipoVenta>().AnyAsync(x => x.Nombre == nombre && x.Id != idToIgnore); 
        }

    }
}