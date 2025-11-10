using AutoMapper;
using PlazaCore.Entites;
using Shared.DTO.InsightHotel;
using System.Text.Json;

namespace Plaza.Mapping.InsightHotelMap
{
    public class InsightHotelMapping : Profile
    {
        public InsightHotelMapping()
        {
            CreateMap<InsightHotel, InsightHotelDto>()
                .ForMember(dest => dest.RestaurantImageIds, opt => opt.MapFrom(src => DeserializeIds(src.RestaurantImages)))
                .ForMember(dest => dest.CafeImageIds, opt => opt.MapFrom(src => DeserializeIds(src.CafeImages)))
                .ForMember(dest => dest.Facilities, opt => opt.MapFrom(src => src.Facilities));

            CreateMap<InsightHotelDto, InsightHotel>()
                .ForMember(dest => dest.RestaurantImages, opt => opt.MapFrom(src => SerializeIds(src.RestaurantImageIds)))
                .ForMember(dest => dest.CafeImages, opt => opt.MapFrom(src => SerializeIds(src.CafeImageIds)))
                .ForMember(dest => dest.Facilities, opt => opt.MapFrom(src => src.Facilities));
        }

        private static List<int> DeserializeIds(string? json)
        {
            if (string.IsNullOrEmpty(json)) return new List<int>();
            return JsonSerializer.Deserialize<List<int>>(json) ?? new List<int>();
        }

        private static string SerializeIds(List<int> ids)
        {
            return JsonSerializer.Serialize(ids ?? new List<int>());
        }
    }
}