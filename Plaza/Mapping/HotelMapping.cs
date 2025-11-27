using AutoMapper;
using PlazaCore.Entites;
using Shared.DTO.Hotel;
using Shared.Security;
using Shared.ImagePathGeneration; 
using System.Text.Json;
using Shared.DTO.User;
using Shared.Helper;

public class HotelMapping : Profile
{
    public HotelMapping()
    {
        CreateMap<Hotel, HotelDto>()
            .ForMember(dest => dest.Images,
                       opt => opt.MapFrom(src => Methods.ConvertIdsToFileDTO(src.ImageIds, "hotel")))
            .ForMember(dest => dest.Logo,
                opt => opt.MapFrom(src =>
               Methods.ConvertIdsToFileDTO(src.LogoId  , "logo").FirstOrDefault()))
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => IdEncoder.EncodeId(src.Id)))
            .ForMember(dest => dest.Insight, opt => opt.MapFrom(src => src.InsightHotel));

        CreateMap<CreateHotelDto, Hotel>()
            .ForMember(dest => dest.ImageIds,
                  opt => opt.MapFrom(src => SerializeIds(src.ImageIds)))
            .ForMember(dest => dest.LogoId,
                  opt => opt.MapFrom(src => SerializeIds(new List<int>() { src.LogoId })));


        CreateMap<UpdateHotelDTO, Hotel>()
            .ForMember(dest => dest.ImageIds,
               opt => opt.MapFrom(src => SerializeIds(src.ImageIds)))
            .ForMember(dest => dest.LogoId,
                  opt => opt.MapFrom(src => SerializeIds(new List<int>() { src.LogoId })));
    }

    // 🧩 Helper methods

    #region old One
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
    //        var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", entityType);

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
    #endregion
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
    private static string SerializeIds(List<int> ids)
    {
        return ids == null ? "[]" : JsonSerializer.Serialize(ids);
    }
}
