using AutoMapper;
using RealStateApp.Core.Application.Interfaces.Repositories;
using RealStateApp.Core.Application.Interfaces.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RealStateApp.Core.Application.Services
{
    public class GenericService<SaveDTO, DTO, Model> : IGenericService<SaveDTO, DTO, Model>
         where SaveDTO : class
         where DTO : class
         where Model : class
    {
        private readonly IGenericRepository<Model> _repository;
        private readonly IMapper _mapper;

        public GenericService(IGenericRepository<Model> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public virtual async Task<SaveDTO> AddAsync(SaveDTO dto)
        {
            Model entity = _mapper.Map<Model>(dto);
            entity = await _repository.AddAsync(entity);
            return _mapper.Map<SaveDTO>(entity);
        }

        public virtual async Task UpdateAsync(SaveDTO dto, int id)
        {
            Model entity = _mapper.Map<Model>(dto);
            await _repository.UpdateAsync(entity, id);
        }

        public virtual async Task DeleteAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            await _repository.DeleteAsync(entity);
        }

        public virtual async Task<List<DTO>> GetAllDTOsAsync()
        {
            var entityList = await _repository.GetAllAsync();
            return _mapper.Map<List<DTO>>(entityList);
        }

        public virtual async Task<SaveDTO> GetByIdSaveDTOAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            return _mapper.Map<SaveDTO>(entity);
        }
    }
}