using System.Text.Json;
using AutoMapper;
using PlazaCore.Entites;
using Shared.DTO.Rooms;
using Shared.Enums;
using Shared.Helper;
using Shared.ImagePathGeneration;
using Shared.Security;

namespace Plaza.Mapping.Rooms
{
    public class RoomMapping : Profile
    {
        public RoomMapping()
        {
            CreateMap<Room, CreateRoomDTO>();
               

            CreateMap<CreateRoomDTO, Room>()
               .ForMember(dest => dest.Hotel, opt => opt.Ignore())
               .ForMember(dest => dest.ImageIds, opt => opt.MapFrom(src => SerializeIds(src.ImageIds)))
               .ForMember(dest => dest.HotelId, opt => opt.MapFrom(src => IdEncoder.DecodeId(src.HotelId)));



            CreateMap<UpdateRoomDTO, Room>()
               .ForMember(dest => dest.Hotel, opt => opt.Ignore())
               .ForMember(dest => dest.ImageIds, opt => opt.MapFrom(src => SerializeIds(src.ImageIds)))
               .ForMember(dest => dest.HotelId, opt => opt.MapFrom(src => IdEncoder.DecodeId(src.HotelId)));

            CreateMap<Room, RoomDTO>().
                ForMember(dest => dest.HotelName, opt => opt.MapFrom(src => src.Hotel.Name))
               .ForMember(dest => dest.Images, opt => opt.MapFrom(src => Methods.ConvertIdsToFileDTO(src.ImageIds, "room")))
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => IdEncoder.EncodeId(src.Id)));
            
        }
        #region old One
        //private static List<ImageDTO> ConvertIdsToImagesDTO(string imageIdsJson, string entityType = "general")
        //{
        //    if (string.IsNullOrEmpty(imageIdsJson))
        //        return new List<ImageDTO>();

        //    try
        //    {
        //        var ids = JsonSerializer.Deserialize<List<int>>(imageIdsJson);
        //        var baseUrl = "http://plazainn.runasp.net";
        //        //return ids?.Select(id => $"{baseUrl}/images/{entityType}/{id}.jpg").ToList() ?? new List<string>();
        //        return ids?.Select(id => new ImageDTO
        //        {
        //            Id = id,
        //            Url = $"{baseUrl}/images/{entityType}/{id}.jpg"
        //        }).ToList() ?? new List<ImageDTO>();
        //    }
        //    catch
        //    {
        //        return new List<ImageDTO>();
        //    }
        //}
        //private static List<ImageDTO> ConvertIdsToImagesDTO(string imageIdsJson, string entityType = "general")
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

       
        private static string SerializeIds(List<int> ids)
        {
            return ids == null ? "[]" : JsonSerializer.Serialize(ids);
        }
    }
}
