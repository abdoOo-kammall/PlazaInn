using AutoMapper;
using PlazaCore.Entites;
using Shared.DTO.InsightHotel;
using Shared.Security;
using System.Text.Json;

namespace Plaza.Mapping.InsightHotelMap
{
    public class InsightHotelMapping : Profile
    {
        public InsightHotelMapping()
        {
            // Entity -> DTO
            CreateMap<InsightHotel, InsightHotelDto>()
                .ForMember(dest => dest.RestaurantImageIds,
                           opt => opt.MapFrom(src => DeserializeIds(src.RestaurantImages)))
                .ForMember(dest => dest.CafeImageIds,
                           opt => opt.MapFrom(src => DeserializeIds(src.CafeImages)))
                .ForMember(dest => dest.Facilities,
                           opt => opt.MapFrom(src => src.Facilities ?? new Dictionary<string, bool>()));

            // DTO -> Entity
            CreateMap<InsightHotelDto, InsightHotel>()
                .ForMember(dest => dest.RestaurantImages,
                           opt => opt.MapFrom(src => SerializeIds(src.RestaurantImageIds)))
                .ForMember(dest => dest.CafeImages,
                           opt => opt.MapFrom(src => SerializeIds(src.CafeImageIds)))
                .ForMember(dest => dest.Facilities,
                           opt => opt.MapFrom(src => src.Facilities ?? new Dictionary<string, bool>()));

            // Create DTO -> Entity
            CreateMap<CreateInsightHotelDto, InsightHotel>()
                
                .ForMember(dest => dest.RestaurantImages,
                           opt => opt.MapFrom(src => SerializeIds(src.RestaurantImageIds)))
                .ForMember(dest => dest.CafeImages,
                           opt => opt.MapFrom(src => SerializeIds(src.CafeImageIds)))
                .ForMember(dest => dest.Facilities,
                           opt => opt.MapFrom(src => src.Facilities ?? new Dictionary<string, bool>()));

            // Update DTO -> Entity
            CreateMap<UpdateInsightHotelDto, InsightHotel>()
                
                .ForMember(dest => dest.RestaurantImages,
                           opt => opt.MapFrom(src => SerializeIds(src.RestaurantImageIds)))
                .ForMember(dest => dest.CafeImages,
                           opt => opt.MapFrom(src => SerializeIds(src.CafeImageIds)))
                .ForMember(dest => dest.Facilities,
                           opt => opt.MapFrom(src => src.Facilities ?? new Dictionary<string, bool>()));
        }

        // Helper methods
        private static List<int> DeserializeIds(string? json)
        {
            if (string.IsNullOrEmpty(json)) return new List<int>();
            try
            {
                return JsonSerializer.Deserialize<List<int>>(json) ?? new List<int>();
            }
            catch
            {
                return new List<int>();
            }
        }

        private static string SerializeIds(List<int>? ids)
        {
            return JsonSerializer.Serialize(ids ?? new List<int>());
        }
    }
}
