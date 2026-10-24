using AutoMapper;
using PlazaCore.Entites;
using Shared.DTO.Booking;
using Shared.Security;

namespace Plaza.Mapping.BookingMap
{
    public class BookingMapping : Profile
    {
        public BookingMapping()
        {
            // HotelId/RoomId are decoded and validated in BookingService, not here.
            CreateMap<BookingRequestDto, Booking>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.HotelId, opt => opt.Ignore())
                .ForMember(dest => dest.RoomId, opt => opt.Ignore())
                .ForMember(dest => dest.Hotel, opt => opt.Ignore())
                .ForMember(dest => dest.Room, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.TotalPrice, opt => opt.Ignore())
                .ForMember(dest => dest.IsPaid, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.MerchantOrderId, opt => opt.Ignore())
                .ForMember(dest => dest.PaymentReference, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.RoomsBooked, opt => opt.MapFrom(src => src.RoomsCount));

            CreateMap<Booking, BookingResponseDto>()
                .ForMember(dest => dest.EncodedId, opt => opt.MapFrom(src => IdEncoder.EncodeId(src.Id)))
                .ForMember(dest => dest.EncodedHotelId, opt => opt.MapFrom(src => IdEncoder.EncodeId(src.HotelId)))
                .ForMember(dest => dest.EncodedRoomId, opt => opt.MapFrom(src => IdEncoder.EncodeId(src.RoomId)))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
        }
    }
}
