using System.Text.Json;
using AutoMapper;
using PlazaCore.Entites;
using Shared.DTO.AboutUs;
using Shared.DTO.HomeSlider;
using Shared.Helper;

namespace Plaza.Mapping.AboutUsMap
{
    public class AboutUsMapping : Profile
    {
        public AboutUsMapping()
        {
            CreateMap<AboutUs, AboutUsDto>()
                 .ForMember(dest => dest.Images, opt => opt.MapFrom(src => Methods.ConvertIdsToFileDTO(src.ImageIds, "aboutus")));
                 
            CreateMap<CreateAboutUsDto, AboutUs>()
                .ForMember(dest => dest.ImageIds, opt => opt.MapFrom(src => SerializeIds(src.ImagesId)));
            CreateMap<UpdateAboutUsDto, AboutUs>()
                .ForMember(dest => dest.ImageIds, opt => opt.MapFrom(src => SerializeIds(src.ImagesId)));
        }
        private static string SerializeIds(List<int> ids)
        {
            return ids == null ? "[]" : JsonSerializer.Serialize(ids);
        }
    }
}
