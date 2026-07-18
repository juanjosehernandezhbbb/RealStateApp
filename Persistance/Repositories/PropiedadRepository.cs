using Microsoft.EntityFrameworkCore;
using RealStateApp.Core.Application.Interfaces.Repositories;
using RealStateApp.Core.Domain.Entities;
using RealStateApp.Persistance.Contexts;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RealStateApp.Persistance.Repositories
{
    public class PropiedadRepository : GenericRepository<Propiedad>, IPropiedadRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public PropiedadRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public override async Task<List<Propiedad>> GetAllAsync()
        {
            return await _dbContext.Set<Propiedad>()
                .Include(p => p.TipoPropiedad)
                .Include(p => p.TipoVenta)
                .Include(p => p.Agente)
                .Include(p => p.Imagenes)
                .Include(p => p.PropiedadMejoras).ThenInclude(pm => pm.Mejora)
                .ToListAsync();
        }

        public override async Task<Propiedad> GetByIdAsync(int id)
        {
            return await _dbContext.Set<Propiedad>()
                .Include(p => p.TipoPropiedad)
                .Include(p => p.TipoVenta)
                .Include(p => p.Agente)
                .Include(p => p.Imagenes)
                .Include(p => p.PropiedadMejoras).ThenInclude(pm => pm.Mejora)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task DeleteConDependenciasAsync(int propiedadId)
        {
            var mensajes = _dbContext.Set<Mensaje>().Where(m => m.PropiedadId == propiedadId);
            _dbContext.Set<Mensaje>().RemoveRange(mensajes);

            // Luego borramos la propiedad
            var propiedad = await _dbContext.Set<Propiedad>().FindAsync(propiedadId);
            if (propiedad != null)
            {
                _dbContext.Set<Propiedad>().Remove(propiedad);
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task<bool> ExisteCodigoAsync(string codigo)
        {
            return await _dbContext.Set<Propiedad>().AnyAsync(p => p.Codigo == codigo);
        }

    }
}