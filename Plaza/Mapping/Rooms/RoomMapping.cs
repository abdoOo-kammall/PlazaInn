using System.Text.Json;
using AutoMapper;
using PlazaCore.Entites;
using Shared.DTO.Rooms;
using Shared.Helper;
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

        private static string SerializeIds(List<int> ids)
        {
            return ids == null ? "[]" : JsonSerializer.Serialize(ids);
        }
    }
}
