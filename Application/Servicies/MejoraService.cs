using AutoMapper;
using RealStateApp.Core.Application.DTOs.Mejoras;
using RealStateApp.Core.Application.Interfaces.Repositories;
using RealStateApp.Core.Application.Interfaces.Services;
using RealStateApp.Core.Domain.Entities;

namespace RealStateApp.Core.Application.Services
{
    public class MejoraService : GenericService<SaveMejoraDTO, MejoraDTO, Mejora>, IMejoraService
    {
        public MejoraService(IGenericRepository<Mejora> repository, IMapper mapper) : base(repository, mapper)
        {
        }
    }
}