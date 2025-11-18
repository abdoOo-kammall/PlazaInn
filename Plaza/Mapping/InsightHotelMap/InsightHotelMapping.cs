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
        //private static List<ImageDTO> ConvertIdsToImageDTO(string imageIdsJson, string entityType = "general")
        //{
        //    if (string.IsNullOrEmpty(imageIdsJson))
        //        return new List<ImageDTO>();

        //    try
        //    {
        //        var ids = JsonSerializer.Deserialize<List<int>>(imageIdsJson);
        //        if (ids == null || !ids.Any())
        //            return new List<ImageDTO>();

        //        var list = new List<ImageDTO>();
        //        //var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "menus", entityType);
        //        var baseFolder = entityType.ToLower().Contains("menu") ? "menus" : "images";
        //        var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", baseFolder, entityType);


        //        foreach (var id in ids)
        //        {
        //            var matchedFile = Directory.GetFiles(folderPath, $"{id}.*").FirstOrDefault();
        //            if (matchedFile != null)
        //            {
        //                var relativePath = matchedFile.Replace(Directory.GetCurrentDirectory(), "")
        //                                              .Replace("\\", "/")
        //                                              .TrimStart('/');
        //                list.Add(new ImageDTO
        //                {
        //                    Id = id,
        //                    Url = ImagePathCreation.BuildFullImageUrl(relativePath)
        //                });
        //            }
        //        }

        //        return list;
        //    }
        //    catch
        //    {
        //        return new List<ImageDTO>();
        //    }
        //}

    }
}
