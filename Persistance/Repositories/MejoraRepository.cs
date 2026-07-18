using Microsoft.EntityFrameworkCore;
using RealStateApp.Core.Application.Interfaces.Repositories;
using RealStateApp.Core.Domain.Entities;
using RealStateApp.Persistance.Contexts;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RealStateApp.Persistance.Repositories
{
    public class MejoraRepository : GenericRepository<Mejora>, IMejoraRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public MejoraRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public override async Task<List<Mejora>> GetAllAsync() =>
            await _dbContext.Set<Mejora>().Include(m => m.PropiedadMejoras).ToListAsync();

        public override async Task<Mejora> GetByIdAsync(int id) =>
            await _dbContext.Set<Mejora>().Include(m => m.PropiedadMejoras)
                .FirstOrDefaultAsync(m => m.Id == id);

        public async Task<bool> ExisteNombreAsync(string nombre, int? idToIgnore = null)
        {
            return await _dbContext.Set<Mejora>().AnyAsync(x => x.Nombre == nombre && x.Id != idToIgnore);
        }

    }

}