using Shared.DTO.Booking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlazaCore.ServiceContract
{
    public interface IBookingService
    {
        //Task<IReadOnlyList<RoomAvailabilityDto>>GetHotelRoomsAvailabilityAsync( string encodedHotelId,  RoomAvailabilityRequestDto request);
        //Task<decimal> CalculateTotalPriceAsync(BookingRequestDto request);
        //Task<bool> CheckAvailabilityAsync(BookingRequestDto request);
        Task<BookingResponseDto> CreateBookingAsync(BookingRequestDto request);
        Task ConfirmBookingPaymentAsync(string paymentReference);
        Task<BookingResponseDto?> GetBookingByIdAsync(int bookingId);
        Task AttachPaymentReferenceAsync(int bookingId, string paymentReference);

    }

}
