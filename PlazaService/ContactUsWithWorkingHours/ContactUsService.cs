using AutoMapper;
using PlazaCore.Entites;
using PlazaCore.RepositoryContract;
using PlazaCore.ServiceContract;
using PlazaCore.Specification.ContactUsSpec;
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
      
    

        public async Task<ContactUsDto> GetAllContactUs()
        {
            var spec = new ContactUsSpecification();
            var ContactSUs = await _repo.GetAllWithSpecAsync(spec);
            var contactUs = ContactSUs.FirstOrDefault();
            return _mapper.Map<ContactUsDto>(contactUs);


           
        }

    

        public async Task UpdateContactUs(UpdateContactUsDto dto)
        {
            var list = await _repo.GetAllWithSpecAsync(new ContactUsSpecification());
            var entity = list.FirstOrDefault();

            if (entity == null)
            {
                entity = _mapper.Map<ContactUs>(dto);
                await _repo.AddAsync(entity);
                await _repo.SaveChangesAsync();
                return;
            }

            // Update simple fields
            entity.Email = dto.Email;
            entity.Address = dto.Address;
            entity.Phone = dto.Phone;
            entity.Instagram = dto.Instagram;
            entity.Facebook = dto.Facebook;
            entity.WhatsApp = dto.WhatsApp;
            entity.LinkedIn = dto.LinkedIn;

            // ✅ Update WorkingHours smartly
            foreach (var whDto in dto.WorkingHours)
            {
                var existing = entity.WorkingHours
                    .FirstOrDefault(x => x.Day == whDto.Day);

                if (existing != null)
                {
                    // UPDATE
                    existing.OpenTime = whDto.OpenTime;
                    existing.CloseTime = whDto.CloseTime;
                }
                else
                {
                    // ADD
                    entity.WorkingHours.Add(new WorkingHour
                    {
                        Day = whDto.Day,
                        OpenTime = whDto.OpenTime,
                        CloseTime = whDto.CloseTime
                    });
                }
            }

            await _repo.SaveChangesAsync();
        }



    }
}
