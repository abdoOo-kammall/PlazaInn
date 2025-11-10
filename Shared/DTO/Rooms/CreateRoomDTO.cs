using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Shared.DTO.Rooms
{
    public class CreateRoomDTO
    {
        [Required(ErrorMessage = "HotelId is required")]
        public int HotelId { get; set; }

        [Required(ErrorMessage = "Room number is required")]
        [StringLength(10, ErrorMessage = "Room number can't exceed 10 characters")]
        public string RoomNumber { get; set; }

        [Required(ErrorMessage = "Room type is required")]
        public string Type { get; set; }

        [Range(1, double.MaxValue, ErrorMessage = "Price per night must be greater than 0")]
        public decimal PricePerNight { get; set; }
        public Dictionary<string, bool> Features { get; set; } = new();

        public bool IsAvailable { get; set; } = true;

        [MinLength(1, ErrorMessage = "At least one image is required")]
        public List<int> ImageIds { get; set; } = new();
    }
}
