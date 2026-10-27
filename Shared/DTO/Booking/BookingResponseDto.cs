using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DTO.Booking
{
    public class BookingResponseDto
    {
        public string EncodedId { get; set; } = null!; // بدل Id حابب تشفره
        public string BookingType { get; set; } = "Hotel";
        public string? EncodedHotelId { get; set; }
        public string? EncodedRoomId { get; set; }
        public string? EncodedTripId { get; set; }
        public string? EncodedTransportId { get; set; }
        public string? EncodedEventId { get; set; }
        public string? EncodedPackageId { get; set; }
        public string? ItemName { get; set; }
        public string? ItemNameAr { get; set; }
        public string? HotelName { get; set; }
        public string? HotelNameAr { get; set; }
        public string? RoomName { get; set; }
        public string? RoomNameAr { get; set; }
        public DateTime? ServiceDate { get; set; }
        public int Quantity { get; set; }
        public int Adults { get; set; }
        public int Children { get; set; }
        public int RoomsBooked { get; set; }
        public bool ExtraBed { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public string CustomerName { get; set; } = null!;
        public string CustomerEmail { get; set; } = null!;
        public string? CustomerPhone { get; set; }
        public bool IsPaid { get; set; }
        public string? PaymentReference { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime CreatedAt { get; set; }
    }

}
