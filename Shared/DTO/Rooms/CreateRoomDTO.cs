using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Shared.Enums;

namespace Shared.DTO.Rooms
{
    public class CreateRoomDTO
    {
        [Required(ErrorMessage = "HotelId is required")]
        public string HotelId { get; set; }

       

        [Required(ErrorMessage = "Room type is required")]
        public RoomType Type { get; set; }

        [Range(1, double.MaxValue, ErrorMessage = "Price per night must be greater than 0")]
        public decimal PricePerNight { get; set; }
        public Dictionary<string, bool> Features { get; set; } = new();

        public bool IsAvailable { get; set; } = true;

        [MinLength(1, ErrorMessage = "At least one image is required")]
        public List<int> ImageIds { get; set; } = new();
    }
}
