using AutoMapper;
using RealStateApp.Core.Domain.Enums;
using RealStateApp.Core.Application.DTOs.Propiedades;
using RealStateApp.Core.Application.Exceptions;
using RealStateApp.Core.Application.Interfaces.Repositories;
using RealStateApp.Core.Application.Interfaces.Services;
using RealStateApp.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
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

            // 1. Mapear y asignar las Imágenes usando la propiedad de navegación 'Imagenes'
            if (dto.ImagenesUrls != null && dto.ImagenesUrls.Any())
            {
                entity.Imagenes = dto.ImagenesUrls.Select(url => new PropiedadImagen
                {
                    ImagenUrl = url // Si en tu entidad PropiedadImagen la propiedad se llama diferente (ej: RutaImagen), ajusta este nombre
                }).ToList();
            }

            // 2. Mapear y asignar las Mejoras usando la propiedad 'PropiedadMejoras'
            if (dto.MejorasIds != null && dto.MejorasIds.Any())
            {
                entity.PropiedadMejoras = dto.MejorasIds.Select(mejoraId => new PropiedadMejora
                {
                    MejoraId = mejoraId
                }).ToList();
            }

            // Al guardar 'entity', EF Core inserta automáticamente la propiedad y sus registros dependientes
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

            // 1. Actualizar campos primitivos
            propiedad.TipoPropiedadId = dto.TipoPropiedadId;
            propiedad.TipoVentaId = dto.TipoVentaId;
            propiedad.Precio = dto.Precio;
            propiedad.Descripcion = dto.Descripcion;
            propiedad.Tamano = dto.Tamano;
            propiedad.CantidadHabitaciones = dto.CantidadHabitaciones;
            propiedad.CantidadBanos = dto.CantidadBanos;

            // 2. Actualizar Mejoras (reemplazar anteriores por las nuevas seleccionadas)
            propiedad.PropiedadMejoras.Clear();
            if (dto.MejorasIds != null && dto.MejorasIds.Any())
            {
                foreach (var mejoraId in dto.MejorasIds)
                {
                    propiedad.PropiedadMejoras.Add(new PropiedadMejora { PropiedadId = id, MejoraId = mejoraId });
                }
            }

            // 3. Actualizar Imágenes (solo si el usuario subió fotos nuevas)
            if (dto.ImagenesUrls != null && dto.ImagenesUrls.Any())
            {
                propiedad.Imagenes.Clear();
                foreach (var url in dto.ImagenesUrls)
                {
                    propiedad.Imagenes.Add(new PropiedadImagen { PropiedadId = id, ImagenUrl = url });
                }
            }

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

            await _repository.DeleteConDependenciasAsync(id);
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

        public new async Task<List<PropiedadDTO>> GetAllAsync()
        {
            var propiedades = await _repository.GetAllAsync();
            return _mapper.Map<List<PropiedadDTO>>(propiedades);
        }
    }
}