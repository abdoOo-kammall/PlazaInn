using System.Text.Json;
using AutoMapper;
using PlazaCore.Entites;
using Shared.DTO.Partner;

namespace Plaza.Mapping.Partner
{
    public class PartnersMapping : Profile
    {
        public PartnersMapping()
        {

            CreateMap<Partners, PartnerDto>()
                .ForMember(dest => dest.HotelsIds, opt => opt.MapFrom(src => src.Hotels.Select(h => h.Id).ToList()));

            CreateMap<CreatePartnerDto, Partners>()
                .ForMember(dest => dest.Hotels, opt => opt.MapFrom(src => src.HotelsIds.Select(id => new { Id = id }).ToList()));

            CreateMap<UpdatePartnerDto, Partners>()
                .ForMember(dest => dest.Hotels, opt => opt.MapFrom(src => src.HotelsIds.Select(id => new { Id = id }).ToList()));

        }
    }
}
