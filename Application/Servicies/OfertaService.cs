using AutoMapper;
using RealStateApp.Core.Application.DTOs.Ofertas;
using RealStateApp.Core.Application.Interfaces.Repositories;
using RealStateApp.Core.Application.Interfaces.Services;
using RealStateApp.Core.Domain.Entities;
using RealStateApp.Core.Domain.Enums;
using RealStateApp.Core.Application.Exceptions;
using System.Linq;

namespace RealStateApp.Core.Application.Services
{
    public class OfertaService : GenericService<SaveOfertaDTO, OfertaDTO, Oferta>, IOfertaService
    {
        private readonly IOfertaRepository _repository;
        private readonly IPropiedadRepository _propiedadRepository;
        private readonly IMapper _mapper;

        public OfertaService(IOfertaRepository repository, IPropiedadRepository propiedadRepository, IMapper mapper) : base(repository, mapper)
        {
            _repository = repository;
            _propiedadRepository = propiedadRepository;
            _mapper = mapper;
        }

        public override async Task<SaveOfertaDTO> AddAsync(SaveOfertaDTO dto)
        {
            var propiedad = await _propiedadRepository.GetByIdAsync(dto.PropiedadId);
            if (propiedad == null) throw new NotFoundException("Propiedad no encontrada.");
            if (propiedad.Estado == EstadoPropiedad.Vendida) throw new ArgumentException("No se pueden hacer ofertas a una propiedad vendida.");

            var ofertasExistentes = await _repository.GetAllAsync();
            bool tieneOfertaPendiente = ofertasExistentes.Any(o => o.PropiedadId == dto.PropiedadId && o.CompradorId == dto.CompradorId && o.Estado == EstadoOferta.Pendiente);

            if (tieneOfertaPendiente) throw new ArgumentException("Ya tienes una oferta pendiente para esta propiedad.");

            Oferta entity = _mapper.Map<Oferta>(dto);
            entity.Estado = EstadoOferta.Pendiente;
            entity.FechaOferta = DateTime.Now;
            entity = await _repository.AddAsync(entity);
            return _mapper.Map<SaveOfertaDTO>(entity);
        }

        public async Task AceptarOfertaAsync(int ofertaId, string agenteId)
        {
            var oferta = await _repository.GetByIdAsync(ofertaId);
            if (oferta == null) throw new NotFoundException("Oferta no encontrada.");
            if (oferta.Estado != EstadoOferta.Pendiente) throw new ArgumentException("La oferta ya fue respondida.");

            var propiedad = await _propiedadRepository.GetByIdAsync(oferta.PropiedadId);
            if (propiedad.AgenteId != agenteId) throw new UnauthorizedAccessException("No eres el agente asignado a esta propiedad.");
            if (propiedad.Estado == EstadoPropiedad.Vendida) throw new ArgumentException("La propiedad ya fue vendida.");

            oferta.Estado = EstadoOferta.Aceptada;
            propiedad.Estado = EstadoPropiedad.Vendida;

            await _repository.UpdateAsync(oferta, oferta.Id);
            await _propiedadRepository.UpdateAsync(propiedad, propiedad.Id);

            var todasLasOfertas = await _repository.GetAllAsync();
            var otrasOfertas = todasLasOfertas.Where(o => o.PropiedadId == oferta.PropiedadId && o.Id != ofertaId && o.Estado == EstadoOferta.Pendiente).ToList();

            foreach (var otra in otrasOfertas)
            {
                otra.Estado = EstadoOferta.Rechazada;
                await _repository.UpdateAsync(otra, otra.Id);
            }
        }

        public async Task RechazarOfertaAsync(int ofertaId, string agenteId)
        {
            var oferta = await _repository.GetByIdAsync(ofertaId);
            if (oferta == null) throw new NotFoundException("Oferta no encontrada.");
            if (oferta.Estado != EstadoOferta.Pendiente) throw new ArgumentException("La oferta ya fue respondida.");

            var propiedad = await _propiedadRepository.GetByIdAsync(oferta.PropiedadId);
            if (propiedad.AgenteId != agenteId) throw new UnauthorizedAccessException("No eres el agente asignado a esta propiedad.");

            oferta.Estado = EstadoOferta.Rechazada;
            await _repository.UpdateAsync(oferta, oferta.Id);
        }
    }
}