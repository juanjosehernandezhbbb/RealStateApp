using AutoMapper;
using RealStateApp.Core.Domain.Enums;
using RealStateApp.Core.Application.DTOs.Propiedades;
using RealStateApp.Core.Application.Exceptions; 
using RealStateApp.Core.Application.Interfaces.Repositories;
using RealStateApp.Core.Application.Interfaces.Services;
using RealStateApp.Core.Domain.Entities;
using System;
using System.Threading.Tasks;

namespace RealStateApp.Core.Application.Services
{
    public class PropiedadService : GenericService<SavePropiedadDTO, PropiedadDTO, Propiedad>, IPropiedadService
    {
        private readonly IPropiedadRepository _repository;
        private readonly IMapper _mapper;

        public PropiedadService(IPropiedadRepository repository, IMapper mapper) : base(repository, mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public override Task UpdateAsync(SavePropiedadDTO dto, int id) =>
            throw new InvalidOperationException("Usa UpdateAsync(dto, id, agenteId) para propiedades.");

        public override Task DeleteAsync(int id) =>
            throw new InvalidOperationException("Usa DeleteAsync(id, agenteId) para propiedades.");

        public override async Task<SavePropiedadDTO> AddAsync(SavePropiedadDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.AgenteId))
                throw new ArgumentException("El ID del Agente es obligatorio para crear una propiedad.");

            Propiedad entity = _mapper.Map<Propiedad>(dto);

            entity.Codigo = await GenerarCodigoUnicoAsync(); 
            entity.Estado = EstadoPropiedad.Disponible;
            entity.FechaCreacion = DateTime.Now;

            entity = await _repository.AddAsync(entity);
            return _mapper.Map<SavePropiedadDTO>(entity);
        }

        public async Task UpdateAsync(SavePropiedadDTO dto, int id, string agenteId)
        {
            if (string.IsNullOrWhiteSpace(agenteId))
                throw new UnauthorizedAccessException("El ID del agente no es válido.");

            var propiedad = await _repository.GetByIdAsync(id);
            if (propiedad == null)
                throw new NotFoundException("Propiedad no encontrada.");

            if (propiedad.AgenteId != agenteId)
                throw new UnauthorizedAccessException("No tiene permisos para modificar esta propiedad.");

            propiedad.TipoPropiedadId = dto.TipoPropiedadId;
            propiedad.TipoVentaId = dto.TipoVentaId;
            propiedad.Precio = dto.Precio;
            propiedad.Descripcion = dto.Descripcion;
            propiedad.Tamano = dto.Tamano;
            propiedad.CantidadHabitaciones = dto.CantidadHabitaciones;
            propiedad.CantidadBanos = dto.CantidadBanos;

            await _repository.UpdateAsync(propiedad, id);
        }

        public async Task DeleteAsync(int id, string agenteId)
        {
            if (string.IsNullOrWhiteSpace(agenteId))
                throw new UnauthorizedAccessException("El ID del agente no es válido.");

            var propiedad = await _repository.GetByIdAsync(id);
            if (propiedad == null)
                throw new NotFoundException("Propiedad no encontrada."); 

            if (propiedad.AgenteId != agenteId)
                throw new UnauthorizedAccessException("No tiene permisos para eliminar esta propiedad.");

            await _repository.DeleteAsync(propiedad);
        }

        private async Task<string> GenerarCodigoUnicoAsync()
        {
            Random random = new Random();
            string codigo;
            do
            {
                codigo = random.Next(100000, 999999).ToString();
            }
            while (await _repository.ExisteCodigoAsync(codigo));

            return codigo;
        }


        public async Task<PropiedadDTO> GetByCodeAsync(string codigo)
         {
            var propiedades = await _repository.GetAllAsync(); 
            var propiedad = propiedades.FirstOrDefault(p => p.Codigo == codigo);
            return _mapper.Map<PropiedadDTO>(propiedad);
        }

        public async Task<List<PropiedadDTO>> GetPropiedadesByAgenteAsync(string agenteId)
        {
            var propiedades = await _repository.GetAllAsync();
            var filtradas = propiedades.Where(p => p.AgenteId == agenteId).ToList();
            return _mapper.Map<List<PropiedadDTO>>(filtradas);
        }

    }
}