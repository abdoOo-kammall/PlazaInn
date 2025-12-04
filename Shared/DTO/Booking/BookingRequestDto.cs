using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DTO.Booking
{
    public class BookingRequestDto
    {
        public string EncodedHotelId { get; set; } = null!;
        public string EncodedRoomId { get; set; } = null!;
        public int Adults { get; set; }
        public int Children { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }

      
        public string CustomerName { get; set; } = null!;
        public string CustomerEmail { get; set; } = null!;
        //public string CustomerPhone { get; set; } = null!;
    }

}
