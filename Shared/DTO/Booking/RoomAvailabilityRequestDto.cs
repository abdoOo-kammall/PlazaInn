using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DTO.Booking
{
    public class RoomAvailabilityRequestDto
    {
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }

        //public int RoomsCount { get; set; } = 1;
        //public int Adults { get; set; }
        //public int Children { get; set; }

        //public bool ExtraBed { get; set; }
        //public int ExtraBedCount { get; set; }
    }
}
