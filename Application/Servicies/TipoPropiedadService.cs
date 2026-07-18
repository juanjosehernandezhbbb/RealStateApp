using AutoMapper;
using RealStateApp.Core.Application.DTOs.TipoPropiedades;
using RealStateApp.Core.Application.Interfaces.Repositories;
using RealStateApp.Core.Application.Interfaces.Services;
using RealStateApp.Core.Domain.Entities;

namespace RealStateApp.Core.Application.Services
{
    public class TipoPropiedadService : GenericService<SaveTipoPropiedadDTO, TipoPropiedadDTO, TipoPropiedad>, ITipoPropiedadService
    {
        private readonly ITipoPropiedadRepository _repository;
        private readonly IMapper _mapper;

        public TipoPropiedadService(ITipoPropiedadRepository repository, IMapper mapper) : base(repository, mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public override async Task<SaveTipoPropiedadDTO> AddAsync(SaveTipoPropiedadDTO dto)
        {
            if (await _repository.ExisteNombreAsync(dto.Nombre))
                throw new ArgumentException("Ya existe un registro con este nombre.");

            return await base.AddAsync(dto);
        }

        public override async Task UpdateAsync(SaveTipoPropiedadDTO dto, int id)
        {
            if (await _repository.ExisteNombreAsync(dto.Nombre, id))
                throw new ArgumentException("Ya existe otro registro con este nombre.");

            await base.UpdateAsync(dto, id);
        }
    }
}