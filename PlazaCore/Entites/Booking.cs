using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.Enums;

namespace PlazaCore.Entites
{
    public class Booking : BaseEntity
    {
        [ForeignKey("Hotel")]
        public int HotelId { get; set; }
        public Hotel Hotel { get; set; }
        [ForeignKey("Room")]

        public int RoomId { get; set; }
        public Room Room { get; set; }

        public string CustomerName { get; set; } = null!;
        public string CustomerEmail { get; set; } = null!;
        //public string CustomerPhone { get; set; } = null!;
        public int RoomsBooked { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public int Adults { get; set; }
        public int Children { get; set; }
        public decimal TotalPrice { get; set; }
        public bool IsPaid { get; set; } = false;
        public string? MerchantOrderId { get; set; }
        public string? PaymentReference { get; set; }
        public BookingStatus Status { get; set; } = BookingStatus.Pending;
    }
}
