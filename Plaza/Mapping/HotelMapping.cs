using AutoMapper;
using PlazaCore.Entites;
using Shared.DTO.Hotel;
using Shared.DTO.Image;
using Shared.Security;
using Shared.ImagePathGeneration; 
using System.Text.Json;

public class HotelMapping : Profile
{
    public HotelMapping()
    {
        CreateMap<Hotel, HotelDto>()
            .ForMember(dest => dest.Images,
                       opt => opt.MapFrom(src => ConvertIdsToImageDTO(src.ImageIds, "hotel")))
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => IdEncoder.EncodeId(src.Id)))
            .ForMember(dest => dest.NumOfRooms,
               opt => opt.MapFrom(src => src.Rooms.Count(r => r.Type != Shared.Enums.RoomType.Suite)))
            .ForMember(dest => dest.NumOfSuites,
               opt => opt.MapFrom(src => src.Rooms.Count(r => r.Type == Shared.Enums.RoomType.Suite)));

        CreateMap<CreateHotelDto, Hotel>()
            .ForMember(dest => dest.ImageIds,
                       opt => opt.MapFrom(src => SerializeIds(src.ImageIds)));

        CreateMap<UpdateHotelDTO, Hotel>()
            .ForMember(dest => dest.ImageIds,
                       opt => opt.MapFrom(src => SerializeIds(src.ImageIds)));
    }

    // 🧩 Helper methods

    //private static List<ImageDTO> ConvertIdsToImageDTO(string imageIdsJson, string entityType = "general")
    //{
    //    if (string.IsNullOrEmpty(imageIdsJson))
    //        return new List<ImageDTO>();

    //    try
    //    {
    //        var ids = JsonSerializer.Deserialize<List<int>>(imageIdsJson);

    //        return ids?.Select(id =>
    //        {
    //            var relativePath = $"wwwroot/images/{entityType}/{id}.jpg";
    //            return new ImageDTO
    //            {
    //                Id = id,
    //                Url = ImagePathCreation.BuildFullImageUrl(relativePath) 
    //            };
    //        }).ToList() ?? new List<ImageDTO>();
    //    }
    //    catch
    //    {
    //        return new List<ImageDTO>();
    //    }
    //}

    private static List<ImageDTO> ConvertIdsToImageDTO(string imageIdsJson, string entityType = "general")
    {
        if (string.IsNullOrEmpty(imageIdsJson))
            return new List<ImageDTO>();

        try
        {
            var ids = JsonSerializer.Deserialize<List<int>>(imageIdsJson);
            if (ids == null || !ids.Any())
                return new List<ImageDTO>();

            var list = new List<ImageDTO>();
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", entityType);

            foreach (var id in ids)
            {
                var matchedFile = Directory.GetFiles(folderPath, $"{id}.*").FirstOrDefault();
                if (matchedFile != null)
                {
                    var relativePath = matchedFile.Replace(Directory.GetCurrentDirectory(), "")
                                                  .Replace("\\", "/")
                                                  .TrimStart('/');
                    list.Add(new ImageDTO
                    {
                        Id = id,
                        Url = ImagePathCreation.BuildFullImageUrl(relativePath)
                    });
                }
            }

            return list;
        }
        catch
        {
            return new List<ImageDTO>();
        }
    }

    private static string SerializeIds(List<int> ids)
    {
        return ids == null ? "[]" : JsonSerializer.Serialize(ids);
    }
}
