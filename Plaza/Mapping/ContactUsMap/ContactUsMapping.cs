using AutoMapper;
using PlazaCore.Entites;
using PlazaRepository.Data.Migrations;
using Shared.DTO.ContactUs;
using Shared.DTO.WorkingHours;

namespace Plaza.Mapping.ContactUsMap
{
    public class ContactUsMappingL:Profile
    {
        public ContactUsMappingL()
        {
            CreateMap<ContactUs, ContactUsDto>()
            .ForMember(dest => dest.WorkingHours,
              opt => opt.MapFrom(src => src.WorkingHours));

            CreateMap<WorkingHour, WorkingHourDto>();

            CreateMap<CreateContactUsDto, ContactUs>();
            CreateMap<CreateWorkingHourDto, WorkingHour>();


        }
    }
}
