using Microsoft.EntityFrameworkCore;
using RealStateApp.Core.Application.Interfaces.Repositories;
using RealStateApp.Core.Domain.Entities;
using RealStateApp.Persistance.Contexts;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RealStateApp.Persistance.Repositories
{
    public class MensajeRepository : GenericRepository<Mensaje>, IMensajeRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public MensajeRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public override async Task<List<Mensaje>> GetAllAsync()
        {
            return await _dbContext.Set<Mensaje>()
                .Include(m => m.Cliente)
                .Include(m => m.Agente)
                .ToListAsync();
        }

        public override async Task<Mensaje> GetByIdAsync(int id)
        {
            return await _dbContext.Set<Mensaje>()
                .Include(m => m.Cliente)
                .Include(m => m.Agente)
                .FirstOrDefaultAsync(m => m.Id == id);
        }
    }
}