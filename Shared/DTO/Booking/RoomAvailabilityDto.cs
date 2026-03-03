using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.DTO.File;

namespace Shared.DTO.Booking
{
    public class RoomAvailabilityDto
    {
        public string EncodedRoomId { get; set; } = null!;
        public string? Name { get; set; }
        public string? NameAr { get; set; }
        public decimal PricePerNight { get; set; }
        public bool ExtraBedAvailable { get; set; }
        //public decimal PriceForExtraBed { get; set; }
        public int MaxSleep { get; set; }

        // availability for the requested period:
        public bool IsAvailable { get; set; }
        //public int RoomsLeftForHotelLimit { get; set; } // how many rooms left before hitting hotel's limit
        //public decimal EstimatedTotalPriceForRequest { get; set; } // optional quick calc if client provided roomsCount, extraBedCount
        public List<FileDto> Images { get; set; } = new List<FileDto>();
    }
}
