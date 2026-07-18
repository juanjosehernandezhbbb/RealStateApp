using AutoMapper;
using RealStateApp.Core.Application.DTOs.Mensajes;
using RealStateApp.Core.Application.Interfaces.Repositories;
using RealStateApp.Core.Application.Interfaces.Services;
using RealStateApp.Core.Domain.Entities;
using System;
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

    }
}