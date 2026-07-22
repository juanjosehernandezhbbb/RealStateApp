using AutoMapper;
using RealStateApp.Core.Application.DTOs.Mensajes;
using RealStateApp.Core.Application.Interfaces.Repositories;
using RealStateApp.Core.Application.Interfaces.Services;
using RealStateApp.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RealStateApp.Core.Application.Services
{
    public class MensajeService : GenericService<SaveMensajeDTO, MensajeDTO, Mensaje>, IMensajeService
    {
        private readonly IMensajeRepository _repository;
        private readonly IMapper _mapper;

        public MensajeService(IMensajeRepository repository, IMapper mapper) : base(repository, mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public override async Task<SaveMensajeDTO> AddAsync(SaveMensajeDTO dto)
        {
            if (dto.EmisorId != dto.ClienteId && dto.EmisorId != dto.AgenteId)
                throw new UnauthorizedAccessException("El emisor del mensaje debe ser el cliente o el agente involucrado.");

            Mensaje entity = _mapper.Map<Mensaje>(dto);
            entity.FechaEnvio = DateTime.Now;
            entity = await _repository.AddAsync(entity);
            return _mapper.Map<SaveMensajeDTO>(entity);
        }

        // Implementación requerida por la interfaz
        public async Task<List<MensajeDTO>> GetMensajesPorPropiedadAsync(int propiedadId, string clienteId)
        {
            var todosLosMensajes = await _repository.GetAllAsync();

            var mensajesFiltrados = todosLosMensajes
                .Where(m => m.PropiedadId == propiedadId && m.ClienteId == clienteId)
                .OrderBy(m => m.FechaEnvio) // Ordenamos del más antiguo al más reciente para el chat
                .ToList();

            return _mapper.Map<List<MensajeDTO>>(mensajesFiltrados);
        }

        // Implementación requerida por la interfaz
        public async Task<List<MensajeDTO>> GetMensajesPorAgenteAsync(string agenteId)
        {
            var todosLosMensajes = await _repository.GetAllAsync();

            var mensajesFiltrados = todosLosMensajes
                .Where(m => m.AgenteId == agenteId)
                .OrderByDescending(m => m.FechaEnvio) // Ordenamos del más reciente al más antiguo para listados
                .ToList();

            return _mapper.Map<List<MensajeDTO>>(mensajesFiltrados);
        }

        // Implementación para obtener todos los mensajes de una propiedad (útil para el agente)
        public async Task<List<MensajeDTO>> GetMensajesPorPropiedadAsync(int propiedadId)
        {
            var todosLosMensajes = await _repository.GetAllAsync();

            var mensajesFiltrados = todosLosMensajes
                .Where(m => m.PropiedadId == propiedadId)
                .OrderBy(m => m.FechaEnvio)
                .ToList();

            return _mapper.Map<List<MensajeDTO>>(mensajesFiltrados);
        }
    }
}