using System;
using Shared.Enums;

namespace Shared.DTO.Booking
{
    public class BookingAdminFilterDto
    {
        public int? HotelId { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public BookingStatus? Status { get; set; }
        public BookingType? BookingType { get; set; }
    }
}

