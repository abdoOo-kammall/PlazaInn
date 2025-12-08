using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using PlazaCore.Entites;
using PlazaCore.RepositoryContract;
using PlazaCore.ServiceContract;
using Shared.DTO.AboutUs;

namespace PlazaService.AboutUsImages
{
    public class AboutUsService : IAboutUsService
    {
        private readonly IGenericRepository<AboutUs> _repo;
        private readonly IMapper _mapper;

        public AboutUsService(IGenericRepository<AboutUs> repository ,IMapper mapper)
        {
            this._repo = repository;
            this._mapper = mapper;
        }
        public async Task addAboutUsImages(CreateAboutUsDto aboutUs)
        {
            var aboutUsImages =  _mapper.Map<AboutUs>(aboutUs);
            await _repo.AddAsync(aboutUsImages);
            await _repo.SaveChangesAsync();
        }

        public async Task deleteAboutUsImages(int id)
        {
            var aboutUsImages = await _repo.GetByIdAsync(id);
            if (aboutUsImages != null) {
                _repo.Delete(aboutUsImages);
                await _repo.SaveChangesAsync();
                
            }
        }

        public async Task<AboutUsDto> getAboutUsImagesById(int id)
        {

            var aboutUsImage =  await _repo.GetByIdAsync(id);
            return _mapper.Map<AboutUsDto>(aboutUsImage);

        }

        public async Task<IEnumerable<AboutUsDto>> getAllAboutUsImages()
        {
            var aboutUsImages = await _repo.GetAllAsync();
            return _mapper.Map<IEnumerable< AboutUsDto>>(aboutUsImages);
        }

        public async Task updateAboutUsImages(int id , UpdateAboutUsDto aboutUs)
        {
            var aboutUsImage = await _repo.GetByIdAsync(id);
            if ( aboutUsImage != null)
            {
                 _mapper.Map(aboutUs , aboutUsImage);
                _repo.Update(aboutUsImage);
                await _repo.SaveChangesAsync();
            }
        }

       
    }
}
