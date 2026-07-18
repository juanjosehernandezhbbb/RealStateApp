using AutoMapper;
using RealStateApp.Core.Application.DTOs.Mejoras;
using RealStateApp.Core.Application.Interfaces.Repositories;
using RealStateApp.Core.Application.Interfaces.Services;
using RealStateApp.Core.Domain.Entities;

namespace RealStateApp.Core.Application.Services
{
    public class MejoraService : GenericService<SaveMejoraDTO, MejoraDTO, Mejora>, IMejoraService
    {
        private readonly IMejoraRepository _repository;
        private readonly IMapper _mapper;

        public MejoraService(IMejoraRepository repository, IMapper mapper) : base(repository, mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public override async Task<SaveMejoraDTO> AddAsync(SaveMejoraDTO dto)
        {
            if (await _repository.ExisteNombreAsync(dto.Nombre))
                throw new ArgumentException("Ya existe un registro con este nombre.");

            return await base.AddAsync(dto);
        }

        public override async Task UpdateAsync(SaveMejoraDTO dto, int id)
        {
            if (await _repository.ExisteNombreAsync(dto.Nombre, id))
                throw new ArgumentException("Ya existe otro registro con este nombre.");

            await base.UpdateAsync(dto, id);
        }
    }
}