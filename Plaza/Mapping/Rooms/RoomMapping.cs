using System.Text.Json;
using AutoMapper;
using PlazaCore.Entites;
using Shared.DTO.Rooms;
using Shared.Enums;

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
            ForMember(dest=> dest.HotelName,opt => opt.MapFrom(src => src.Hotel.Name))
           .ForMember(dest => dest.ImageUrls, opt => opt.MapFrom(src => ConvertIdsToUrls(src.ImageIds, "room")));
            ;



        }
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
}
