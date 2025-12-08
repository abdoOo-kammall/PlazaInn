using System.Text.Json;
using AutoMapper;
using PlazaCore.Entites;
using Shared.DTO.HomeSlider;
using Shared.Helper;

namespace Plaza.Mapping.HomeSliderMap
{
    public class HomeSliderMapping : Profile
    {
        public HomeSliderMapping()
        {
            CreateMap<HomeSlider, HomeSliderDto>()
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src => Methods.ConvertIdsToFileDTO(src.ImageIds , "homeSlider")));
            CreateMap<CreateHomeSliderDto, HomeSlider>()
                .ForMember(dest => dest.ImageIds , opt => opt.MapFrom(src => SerializeIds(src.ImagesId)));
            CreateMap<UpdateHomeSliderDto, HomeSlider>()
                .ForMember(dest => dest.ImageIds, opt => opt.MapFrom(src => SerializeIds(src.ImagesId)));

           
        }

        private static string SerializeIds(List<int> ids)
        {
            return ids == null ? "[]" : JsonSerializer.Serialize(ids);
        }
    }

}