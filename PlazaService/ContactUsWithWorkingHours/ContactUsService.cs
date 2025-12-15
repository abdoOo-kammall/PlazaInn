using AutoMapper;
using PlazaCore.Entites;
using PlazaCore.RepositoryContract;
using PlazaCore.ServiceContract;
using PlazaRepository.Data.Migrations;
using Shared.DTO.ContactUs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlazaService.ContactUsWithWorkingHours
{
    public class ContactUsService : IContactUsService
    {
        private readonly IGenericRepository<ContactUs> _repo;
        private readonly IMapper _mapper;

        public ContactUsService(IGenericRepository<ContactUs> repository , IMapper mapper)
        {
            this._repo = repository;
            this._mapper = mapper;
        }
        public async Task CreateContactUs(CreateContactUsDto contactUsDto)
        {
            var contactUs = _mapper.Map<ContactUs>(contactUsDto);
            await _repo.AddAsync(contactUs);
            await _repo.SaveChangesAsync();

        }

        //public async Task DeleteContactUs(int id)
        //{
        //    var contactUs = await _repo.GetByIdAsync(id);
        //    if (contactUs != null) {
        //        _repo.Delete(contactUs);
        //        await _repo.SaveChangesAsync();
        //    }
        //}

        public async Task<IEnumerable<ContactUsDto>> GetAllContactUs()
        { 

            var ContactSUs = await _repo.GetAllAsync();
            return _mapper.Map< IEnumerable<ContactUsDto>>(ContactSUs);
        }

        //public async Task<ContactUsDto> GetContactUs(int id)
        //{
        //    var ContactUs = await _repo.GetByIdAsync(id);
        //    return _mapper.Map<ContactUsDto>(ContactUs);
        //}

        public async Task UpdateContactUs(int id, UpdateContactUsDto contactUsDto)
        {
            var contactUs = await _repo.GetByIdAsync(id);
            if (contactUs != null)
            {
                _mapper.Map<ContactUsDto>(contactUs);
                _repo.Update(contactUs);
                await _repo.SaveChangesAsync();
            }
        }
    }
}
