using System.Text.Json;
using AutoMapper;
using PlazaCore.Entites;
using Shared.DTO.Image;
using Shared.DTO.Rooms;
using Shared.Enums;
using Shared.Security;

namespace Plaza.Mapping.Rooms
{
    public class RoomMapping : Profile
    {
        public RoomMapping()
        {
            CreateMap<Room, CreateRoomDTO>()
                            //.ForMember(dest => dest.HotelName, opt => opt.MapFrom(src => src.Hotel.Name))
                .ForMember(dest => dest.Type , src => src.MapFrom(src=> src.Type.ToString()));

            CreateMap<CreateRoomDTO, Room>()
       .ForMember(dest => dest.Type, opt => opt.MapFrom(src => Enum.Parse<RoomType>(src.Type, true)))
       .ForMember(dest => dest.Hotel, opt => opt.Ignore())
       .ForMember(dest => dest.ImageIds, opt => opt.MapFrom(src => SerializeIds(src.ImageIds)));



            CreateMap<UpdateRoomDTO, Room>();
            CreateMap<Room, RoomDTO>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString())).
            ForMember(dest => dest.HotelName, opt => opt.MapFrom(src => src.Hotel.Name))
           .ForMember(dest => dest.Images, opt => opt.MapFrom(src => ConvertIdsToImagesDTO(src.ImageIds, "room")))
           .ForMember(dest => dest.Id, opt => opt.MapFrom(src => IdEncoder.EncodeId(src.Id)));
            ;



        }
        private static List<ImageDTO> ConvertIdsToImagesDTO(string imageIdsJson, string entityType = "general")
        {
            if (string.IsNullOrEmpty(imageIdsJson))
                return new List<ImageDTO>();

            try
            {
                var ids = JsonSerializer.Deserialize<List<int>>(imageIdsJson);
                var baseUrl = "http://plazainn.runasp.net";
                //return ids?.Select(id => $"{baseUrl}/images/{entityType}/{id}.jpg").ToList() ?? new List<string>();
                return ids?.Select(id => new ImageDTO
                {
                    Id = id,
                    Url = $"{baseUrl}/images/{entityType}/{id}.jpg"
                }).ToList() ?? new List<ImageDTO>();
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
}
