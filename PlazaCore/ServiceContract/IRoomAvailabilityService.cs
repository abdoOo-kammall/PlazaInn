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

        Task<bool> CheckAvailabilityAsync(BookingRequestDto request);

        Task<decimal> CalculateTotalPriceAsync(BookingRequestDto request);
        bool IsDateRangeValid(RoomAvailabilityRequestDto request);

    }
}
