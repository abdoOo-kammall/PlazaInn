using AutoMapper;
using PlazaCore.Entites;
using Shared.DTO.Booking;
using Shared.Enums;
using Shared.Security;

namespace Plaza.Mapping.BookingMap
{
    public class BookingMapping : Profile
    {
        public BookingMapping()
        {
            // HotelId/RoomId and catalog ids are decoded and validated in BookingService, not here.
            CreateMap<BookingRequestDto, Booking>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.BookingType, opt => opt.Ignore())
                .ForMember(dest => dest.HotelId, opt => opt.Ignore())
                .ForMember(dest => dest.RoomId, opt => opt.Ignore())
                .ForMember(dest => dest.Hotel, opt => opt.Ignore())
                .ForMember(dest => dest.Room, opt => opt.Ignore())
                .ForMember(dest => dest.TripId, opt => opt.Ignore())
                .ForMember(dest => dest.Trip, opt => opt.Ignore())
                .ForMember(dest => dest.TransportId, opt => opt.Ignore())
                .ForMember(dest => dest.Transport, opt => opt.Ignore())
                .ForMember(dest => dest.EventId, opt => opt.Ignore())
                .ForMember(dest => dest.Event, opt => opt.Ignore())
                .ForMember(dest => dest.PackageId, opt => opt.Ignore())
                .ForMember(dest => dest.Package, opt => opt.Ignore())
                .ForMember(dest => dest.ServiceDate, opt => opt.Ignore())
                .ForMember(dest => dest.Quantity, opt => opt.Ignore())
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
                .ForMember(dest => dest.BookingType, opt => opt.MapFrom(src => src.BookingType.ToString()))
                .ForMember(dest => dest.EncodedHotelId, opt => opt.MapFrom(src => EncodeOptional(src.HotelId)))
                .ForMember(dest => dest.EncodedRoomId, opt => opt.MapFrom(src => EncodeOptional(src.RoomId)))
                .ForMember(dest => dest.EncodedTripId, opt => opt.MapFrom(src => EncodeOptional(src.TripId)))
                .ForMember(dest => dest.EncodedTransportId, opt => opt.MapFrom(src => EncodeOptional(src.TransportId)))
                .ForMember(dest => dest.EncodedEventId, opt => opt.MapFrom(src => EncodeOptional(src.EventId)))
                .ForMember(dest => dest.EncodedPackageId, opt => opt.MapFrom(src => EncodeOptional(src.PackageId)))
                .ForMember(dest => dest.ItemName, opt => opt.MapFrom(src => ItemName(src)))
                .ForMember(dest => dest.ItemNameAr, opt => opt.MapFrom(src => ItemNameAr(src)))
                .ForMember(dest => dest.HotelName, opt => opt.MapFrom(src => src.Hotel != null ? src.Hotel.Name : null))
                .ForMember(dest => dest.HotelNameAr, opt => opt.MapFrom(src => src.Hotel != null ? src.Hotel.NameAr : null))
                .ForMember(dest => dest.RoomName, opt => opt.MapFrom(src => src.Room != null ? src.Room.Name : null))
                .ForMember(dest => dest.RoomNameAr, opt => opt.MapFrom(src => src.Room != null ? src.Room.NameAr : null))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
        }

        private static string? EncodeOptional(int? id) => id.HasValue ? IdEncoder.EncodeId(id.Value) : null;

        private static string? ItemName(Booking booking) => booking.BookingType switch
        {
            BookingType.Trip => booking.Trip?.Name,
            BookingType.Transport => booking.Transport?.Name,
            BookingType.Event => booking.Event?.Name,
            BookingType.Package => booking.Package?.Name,
            _ => booking.Hotel?.Name
        };

        private static string? ItemNameAr(Booking booking) => booking.BookingType switch
        {
            BookingType.Trip => booking.Trip?.NameAr,
            BookingType.Transport => booking.Transport?.NameAr,
            BookingType.Event => booking.Event?.NameAr,
            BookingType.Package => booking.Package?.NameAr,
            _ => booking.Hotel?.NameAr
        };
    }
}
