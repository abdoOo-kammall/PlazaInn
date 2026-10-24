using AutoMapper;
using PlazaCore.Entites;
using Shared.DTO.Booking;
using Shared.Helper;
using Shared.Security;

namespace Plaza.Mapping.RoomAvailable
{
    public class RoomAvailabiltyMapping : Profile
    {
        public RoomAvailabiltyMapping()
        {
            CreateMap<Room, RoomAvailabilityDto>()
             .ForMember(dest => dest.EncodedRoomId, opt => opt.MapFrom(src => IdEncoder.EncodeId(src.Id)))
             .ForMember(dest => dest.Images, opt => opt.MapFrom(src => Methods.ConvertIdsToFileDTO(src.ImageIds, "room")))
             .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
             .ForMember(dest => dest.NameAr, opt => opt.MapFrom(src => src.NameAr))
             .ForMember(dest => dest.PricePerNight, opt => opt.MapFrom(src => src.PricePerNight))
             .ForMember(dest => dest.PriceForExtraBed, opt => opt.MapFrom(src => src.PriceForExtraBed))
             .ForMember(dest => dest.MaxSleep, opt => opt.MapFrom(src => src.MaxSleep))
             .ForMember(dest => dest.ExtraBedAvailable, opt => opt.MapFrom(src => src.ExtraBed))
             .ForMember(dest => dest.IsAvailable, opt => opt.Ignore())
             .ForMember(dest => dest.AvailableRooms, opt => opt.Ignore())
             .AfterMap((src, dest, ctx) =>
             {
                 var availableRooms = (int)ctx.Items["availableRooms"];
                 var roomsCount = (int)ctx.Items["roomsCount"];
                 dest.AvailableRooms = availableRooms;
                 dest.IsAvailable = availableRooms >= roomsCount;
             });
        }
    }
}
