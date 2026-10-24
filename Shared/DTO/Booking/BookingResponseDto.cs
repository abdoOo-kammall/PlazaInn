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
        public string EncodedHotelId { get; set; } = null!;
        public string EncodedRoomId { get; set; } = null!;
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
