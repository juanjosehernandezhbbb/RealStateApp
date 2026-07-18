using Microsoft.EntityFrameworkCore;
using RealStateApp.Core.Application.Interfaces.Repositories;
using RealStateApp.Core.Domain.Entities;
using RealStateApp.Persistance.Contexts;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RealStateApp.Persistance.Repositories
{
    public class OfertaRepository : GenericRepository<Oferta>, IOfertaRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public OfertaRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public override async Task<List<Oferta>> GetAllAsync()
        {
            return await _dbContext.Set<Oferta>()
                .Include(o => o.Comprador)
                .ToListAsync();
        }

        public override async Task<Oferta> GetByIdAsync(int id)
        {
            return await _dbContext.Set<Oferta>()
                .Include(o => o.Comprador)
                .FirstOrDefaultAsync(o => o.Id == id);
        }
    }
}