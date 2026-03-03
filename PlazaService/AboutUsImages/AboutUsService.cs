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
     
        
        
        public async Task<IEnumerable<AboutUsDto>> getAllAboutUsImages()
        {
            var aboutUsImages = await _repo.GetAllAsync();
            return _mapper.Map<IEnumerable< AboutUsDto>>(aboutUsImages);
        }

        public async Task updateAboutUsImages(UpdateAboutUsDto aboutUs)
        {
            var entity = await _repo.GetAllAsync();
            var actualEntity = entity.FirstOrDefault();

            if (actualEntity != null)
            {
                _mapper.Map(aboutUs, actualEntity);
                _repo.Update(actualEntity);
                await _repo.SaveChangesAsync();
            }
            else {
               
                await _repo.AddAsync(_mapper.Map<AboutUs>(aboutUs));
                await _repo.SaveChangesAsync();
            
            }
        }

       
    }
}
