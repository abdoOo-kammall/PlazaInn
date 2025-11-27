using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.Enums;

namespace PlazaCore.Entites
{
    public class Booking : BaseEntity
    {
        public int HotelId { get; set; }
        public Hotel Hotel { get; set; }

        public int RoomId { get; set; }
        public Room Room { get; set; }

        public string CustomerName { get; set; } = null!;
        public string CustomerEmail { get; set; } = null!;
        public string CustomerPhone { get; set; } = null!;

        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public int Guests { get; set; }

        public decimal TotalPrice { get; set; }

        public BookingStatus Status { get; set; } = BookingStatus.Pending;
    }
}
