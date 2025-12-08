using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using PlazaCore.Entites;
using PlazaCore.RepositoryContract;
using PlazaCore.ServiceContract;
using Shared.DTO.HomeSlider;

namespace PlazaService.HomeSliders
{
    public class HomeSliderService : IHomeSliderService
    {
        private readonly IGenericRepository<HomeSlider> _repo;
        private readonly IMapper _mapper;

        public HomeSliderService(IGenericRepository<HomeSlider> repository , IMapper mapper)
        {
            this._repo = repository;
            this._mapper = mapper;
        }
        public async Task AddHomeSliderAsync(CreateHomeSliderDto homeSlider)
        {
            var homeSlider02 = _mapper.Map<HomeSlider>(homeSlider);
            await _repo.AddAsync(homeSlider02);
            await _repo.SaveChangesAsync();
        }

        public async Task DeleteHomeSliderAsync(int id)
        {
            var homeSlider = await _repo.GetByIdAsync(id);
            if (homeSlider != null) {
                _repo.Delete(homeSlider);
                await _repo.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<HomeSliderDto>> GetAllHomeSliderAsync()
        {
            var sliders = await _repo.GetAllAsync();
            return _mapper.Map<IEnumerable<HomeSliderDto>>(sliders);
        }



        public async Task<HomeSliderDto?> GetHomeSliderAsync(int id)
        {
            var slider = await _repo.GetByIdAsync(id);
            return _mapper.Map<HomeSliderDto>(slider);
        }

        public async Task UpdateHomeSliderAsync(int id , UpdateHomeSliderDto homeSliderDto)
        {
            var homeSliderEdited = await _repo.GetByIdAsync(id);
            if (homeSliderEdited != null)
            {

                var homeSlider = _mapper.Map(homeSliderDto, homeSliderEdited);
                _repo.Update(homeSlider);
                await _repo.SaveChangesAsync();
            }
            else return;
        }
    }
}
