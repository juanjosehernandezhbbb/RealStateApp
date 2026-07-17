using AutoMapper;
using RealStateApp.Core.Application.DTOs.Favoritos;
using RealStateApp.Core.Application.Interfaces.Repositories;
using RealStateApp.Core.Application.Interfaces.Services;
using RealStateApp.Core.Domain.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RealStateApp.Core.Application.Services
{
    public class FavoritoService : GenericService<SaveFavoritoDTO, FavoritoDTO, Favorito>, IFavoritoService
    {
        private readonly IGenericRepository<Favorito> _repository;
        private readonly IMapper _mapper;

        public FavoritoService(IGenericRepository<Favorito> repository, IMapper mapper) : base(repository, mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public override async Task<SaveFavoritoDTO> AddAsync(SaveFavoritoDTO dto)
        {
            var favoritos = await _repository.GetAllAsync();
            if (favoritos.Any(f => f.UsuarioId == dto.UsuarioId && f.PropiedadFavoritaId == dto.PropiedadFavoritaId))
            {
                return dto; 
            }

            Favorito entity = _mapper.Map<Favorito>(dto);
            entity.FechaAgregado = DateTime.Now;
            entity = await _repository.AddAsync(entity);

            return _mapper.Map<SaveFavoritoDTO>(entity);
        }
    }
}