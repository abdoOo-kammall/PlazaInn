using PlazaCore.Entites;
using Shared.DTO.Booking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlazaCore.ServiceContract
{
    public interface IRoomAvailabilityService
    {
        Task<IReadOnlyList<RoomAvailabilityDto>>
            GetHotelRoomsAvailabilityAsync( string encodedHotelId, RoomAvailabilityRequestDto request);

        Task<bool> CheckAvailabilityAsync(BookingQuoteRequestDto request);

        /// <summary>Rooms of the same inventory pool (suites vs. regular rooms) still free on every night of the stay.</summary>
        Task<int> GetAvailableRoomsAsync(Room room, DateTime checkIn, DateTime checkOut, int? excludeBookingId = null);

        Task<decimal> CalculateTotalPriceAsync(BookingQuoteRequestDto request);
        decimal CalculateTotalPrice(Room room, int roomsCount, bool extraBed, DateTime checkIn, DateTime checkOut);

        void ValidateStay(Room room, BookingQuoteRequestDto request);
        bool IsDateRangeValid(RoomAvailabilityRequestDto request);

    }
}
