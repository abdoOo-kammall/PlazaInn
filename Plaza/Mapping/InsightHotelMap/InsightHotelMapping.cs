using AutoMapper;
using PlazaCore.Entites;
using Shared.DTO.InsightHotel;
using Shared.Helper;
using Shared.ImagePathGeneration;
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
                           opt => opt.MapFrom(src =>Methods.ConvertIdsToFileDTO (src.RestaurantImageIds, "restaurant")))
                .ForMember(dest => dest.RestaurantMenuPdfIds,
                           opt => opt.MapFrom(src => Methods.ConvertIdsToFileDTO(src.RestaurantMenuPdfIds, "restaurantMenu")))
                .ForMember(dest => dest.CafeImageIds,
                           opt => opt.MapFrom(src => Methods.ConvertIdsToFileDTO(src.CafeImageIds, "cafe")))
                .ForMember(dest => dest.CafeMenuPdfIds,
                           opt => opt.MapFrom(src => Methods.ConvertIdsToFileDTO(src.CafeMenuPdfIds, "cafeMenu")))
                .ForMember(dest => dest.Facilities,
                           opt => opt.MapFrom(src => src.Facilities ?? new Dictionary<string, bool>()));




            // Create DTO -> Entity
            CreateMap<CreateInsightHotelDto, InsightHotel>()
                
                .ForMember(dest => dest.RestaurantImageIds,
                           opt => opt.MapFrom(src => SerializeIds(src.RestaurantImageIds)))
                .ForMember(dest => dest.CafeImageIds,
                           opt => opt.MapFrom(src => SerializeIds(src.CafeImageIds)))
                .ForMember(dest => dest.RestaurantMenuPdfIds,
                           opt => opt.MapFrom(src => SerializeIds(src.RestaurantMenuPdfIds)))
                .ForMember(dest => dest.CafeMenuPdfIds,
                           opt => opt.MapFrom(src => SerializeIds(src.CafeMenuPdfIds)))
                .ForMember(dest => dest.Facilities,
                           opt => opt.MapFrom(src => src.Facilities ?? new Dictionary<string, bool>()));

            // Update DTO -> Entity
            CreateMap<UpdateInsightHotelDto, InsightHotel>()

                .ForMember(dest => dest.RestaurantImageIds,
                           opt => opt.MapFrom(src => SerializeIds(src.RestaurantImageIds)))
                .ForMember(dest => dest.CafeImageIds
                ,
                           opt => opt.MapFrom(src => SerializeIds(src.CafeImageIds)))
                .ForMember(dest => dest.RestaurantMenuPdfIds,
                           opt => opt.MapFrom(src => SerializeIds(src.RestaurantMenuPdfIds)))
                .ForMember(dest => dest.CafeMenuPdfIds,
                           opt => opt.MapFrom(src => SerializeIds(src.CafeMenuPdfIds)))
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
