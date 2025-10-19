    using AutoMapper;
    using PlazaCore.Entites;
    using Shared.DTO.Hotel;
    using System.Text.Json;

    public class HotelMapping : Profile
    {
        public HotelMapping()
        {
            // 🔹 من Entity إلى DTO
            CreateMap<Hotel, HotelDto>()
                .ForMember(dest => dest.ImageUrls,
                           opt => opt.MapFrom(src => ConvertIdsToUrls(src.ImageIds,"hotel" )));

            // 🔹 من CreateDto إلى Entity
            CreateMap<CreateHotelDto, Hotel>()
                .ForMember(dest => dest.ImageIds,
                           opt => opt.MapFrom(src => SerializeIds(src.ImageIds)));

            // 🔹 من UpdateDto إلى Entity
            CreateMap<UpdateHotelDTO, Hotel>()
                .ForMember(dest => dest.ImageIds,
                           opt => opt.MapFrom(src => SerializeIds(src.ImageIds)));
        }

    // 🧩 Helper methods

    private static List<string> ConvertIdsToUrls(string imageIdsJson, string entityType = "general")
    {
        if (string.IsNullOrEmpty(imageIdsJson))
            return new List<string>();

        try
        {
            var ids = JsonSerializer.Deserialize<List<int>>(imageIdsJson);
            return ids?.Select(id => $"/images/{entityType}/{id}.jpg").ToList() ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }



    private static string SerializeIds(List<int> ids)
        {
            return ids == null ? "[]" : JsonSerializer.Serialize(ids);
        }
    }
