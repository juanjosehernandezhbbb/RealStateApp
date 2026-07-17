using AutoMapper;
using RealStateApp.Core.Domain.Enums;
using RealStateApp.Core.Application.DTOs.Propiedades;
using RealStateApp.Core.Application.Interfaces.Repositories;
using RealStateApp.Core.Application.Interfaces.Services;
using RealStateApp.Core.Domain.Entities;
using System;
using System.Threading.Tasks;

namespace RealStateApp.Core.Application.Services
{
    public class PropiedadService : GenericService<SavePropiedadDTO, PropiedadDTO, Propiedad>, IPropiedadService
    {
        private readonly IGenericRepository<Propiedad> _repository;
        private readonly IMapper _mapper;

        public PropiedadService(IGenericRepository<Propiedad> repository, IMapper mapper) : base(repository, mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public override async Task<SavePropiedadDTO> AddAsync(SavePropiedadDTO dto)
        {
            Propiedad entity = _mapper.Map<Propiedad>(dto);

            entity.Codigo = GenerarCodigoUnico();
            entity.Estado = EstadoPropiedad.Disponible;
            entity.FechaCreacion = DateTime.Now;

            entity = await _repository.AddAsync(entity);
            return _mapper.Map<SavePropiedadDTO>(entity);
        }

        private string GenerarCodigoUnico()
        {
            Random random = new Random();
            return random.Next(100000, 999999).ToString();
        }
    }
}