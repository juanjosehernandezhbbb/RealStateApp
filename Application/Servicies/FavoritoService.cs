using AutoMapper;
using RealStateApp.Core.Application.DTOs.Favoritos;
using RealStateApp.Core.Application.Interfaces.Repositories;
using RealStateApp.Core.Application.Interfaces.Services;
using RealStateApp.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RealStateApp.Core.Application.Services
{
    public class FavoritoService : GenericService<SaveFavoritoDTO, FavoritoDTO, Favorito>, IFavoritoService
    {
        private readonly IFavoritoRepository _repository;
        private readonly IMapper _mapper;

        public FavoritoService(IFavoritoRepository repository, IMapper mapper) : base(repository, mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public override async Task<SaveFavoritoDTO> AddAsync(SaveFavoritoDTO dto)
        {
            if (await _repository.ExisteFavoritoAsync(dto.UsuarioId, dto.PropiedadFavoritaId))
            {
                return null;
            }

            Favorito entity = _mapper.Map<Favorito>(dto);
            entity.FechaAgregado = DateTime.Now;
            entity = await _repository.AddAsync(entity);

            return _mapper.Map<SaveFavoritoDTO>(entity);
        }

        // Usamos 'new' para satisfacer la interfaz de forma limpia
        public new async Task<List<FavoritoDTO>> GetAllAsync()
        {
            var favoritos = await _repository.GetAllAsync();
            return _mapper.Map<List<FavoritoDTO>>(favoritos);
        }
    }
}