using Shared.Enums;

namespace PlazaCore.Entites
{
    public class Booking : BaseEntity
    {
        public BookingType BookingType { get; set; } = BookingType.Hotel;

        public int? HotelId { get; set; }
        public Hotel? Hotel { get; set; }

        public int? RoomId { get; set; }
        public Room? Room { get; set; }

        public int? TripId { get; set; }
        public Trip? Trip { get; set; }

        public int? TransportId { get; set; }
        public Transport? Transport { get; set; }

        public int? EventId { get; set; }
        public Event? Event { get; set; }

        public int? PackageId { get; set; }
        public Package? Package { get; set; }

        public DateTime? ServiceDate { get; set; }
        public int Quantity { get; set; } = 1;

        public string? UserId { get; set; }

        public string CustomerName { get; set; } = null!;
        public string CustomerEmail { get; set; } = null!;
        public string? CustomerPhone { get; set; }
        public int RoomsBooked { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public int Adults { get; set; }
        public int Children { get; set; }
        public bool ExtraBed { get; set; }
        public decimal TotalPrice { get; set; }
        public bool IsPaid { get; set; } = false;
        public string? MerchantOrderId { get; set; }
        public string? PaymentReference { get; set; }
        public BookingStatus Status { get; set; } = BookingStatus.Pending;
    }
}
