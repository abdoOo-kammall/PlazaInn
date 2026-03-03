//using AutoMapper;
//using PlazaCore.Entites;
//using Shared.DTO.Booking;
//using Shared.Security;

//namespace Plaza.MappingProfiles
//{
//    public class BookingProfile : Profile
//    {
//        public BookingProfile()
//        {
//            // Entity → ResponseDto
//            CreateMap<Booking, BookingResponseDto>()
//                .ForMember(dest => dest.EncodedId,
//                           opt => opt.MapFrom(src => IdEncoder.EncodeId(src.Id)))
//                .ForMember(dest => dest.EncodedHotelId,
//                           opt => opt.MapFrom(src => IdEncoder.EncodeId(src.HotelId)))
//                .ForMember(dest => dest.EncodedRoomId,
//                           opt => opt.MapFrom(src => IdEncoder.EncodeId(src.RoomId)));

//            // RequestDto → Entity
//            CreateMap<BookingRequestDto, Booking>()
//                .ForMember(dest => dest.HotelId,
//                           opt => opt.MapFrom(src => IdEncoder.DecodeId(src.EncodedHotelId)))
//                .ForMember(dest => dest.RoomId,
//                           opt => opt.MapFrom(src => IdEncoder.DecodeId(src.EncodedRoomId)));
//        }
//    }
//}
