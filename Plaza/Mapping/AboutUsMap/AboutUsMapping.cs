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
                 .ForMember(dest => dest.Square1ImageId, opt => opt.MapFrom(src => Methods.ConvertIdsToFileDTO(src.Square1ImageId, "aboutus").FirstOrDefault()))
                 .ForMember(dest => dest.Square2ImageId, opt => opt.MapFrom(src => Methods.ConvertIdsToFileDTO(src.Square2ImageId, "aboutus").FirstOrDefault()))
                 .ForMember(dest => dest.CircleImageId, opt => opt.MapFrom(src => Methods.ConvertIdsToFileDTO(src.CircleImageId, "aboutus").FirstOrDefault()));



            CreateMap<CreateAboutUsDto, AboutUs>()
                .ForMember(dest => dest.Square1ImageId, opt => opt.MapFrom(src => SerializeIds(new List<int>() { src.Square1ImageId })))
                .ForMember(dest => dest.Square2ImageId, opt => opt.MapFrom(src => SerializeIds(new List<int>() { src.Square2ImageId })))
                .ForMember(dest => dest.CircleImageId, opt => opt.MapFrom(src => SerializeIds(new List<int>() { src.CircleImageId })));




            CreateMap<UpdateAboutUsDto, AboutUs>()
                .ForMember(dest => dest.Square1ImageId, opt => opt.MapFrom(src => SerializeIds(new List<int>() { src.Square1ImageId })))
                .ForMember(dest => dest.Square2ImageId, opt => opt.MapFrom(src => SerializeIds(new List<int>() { src.Square2ImageId })))
                .ForMember(dest => dest.CircleImageId, opt => opt.MapFrom(src => SerializeIds(new List<int>() { src.CircleImageId })));
        }
        private static string SerializeIds(List<int> ids)
        {
            return ids == null ? "[]" : JsonSerializer.Serialize(ids);
        }
    }
}
