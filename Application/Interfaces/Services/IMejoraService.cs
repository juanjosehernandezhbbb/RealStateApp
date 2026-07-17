using RealStateApp.Core.Application.DTOs.Mejoras;
using RealStateApp.Core.Domain.Entities;

namespace RealStateApp.Core.Application.Interfaces.Services
{
    public interface IMejoraService : IGenericService<SaveMejoraDTO, MejoraDTO, Mejora>
    {
    }
}