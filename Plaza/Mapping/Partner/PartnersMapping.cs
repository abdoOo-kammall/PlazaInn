using System.Text.Json;
using AutoMapper;
using PlazaCore.Entites;
using Shared.DTO.Partner;
using Shared.Security;

namespace Plaza.Mapping.Partner
{
    public class PartnersMapping : Profile
    {
        public PartnersMapping()
        {

            CreateMap<CreatePartnerDto, Partners>()
                 .ForMember(dest => dest.Hotels, opt => opt.Ignore());

            CreateMap<UpdatePartnerDto, Partners>()
                .ForMember(dest => dest.Hotels, opt => opt.Ignore());

            CreateMap<Partners, PartnerDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => IdEncoder.EncodeId(src.Id)))
                .ForMember(dest => dest.HotelsIds,
                    opt => opt.MapFrom(src => src.Hotels.Select(h => IdEncoder.EncodeId(h.Id)).ToList()));

            CreateMap<Partners, PartnerOnlyDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => IdEncoder.EncodeId(src.Id)));
               


        }
    }
}
