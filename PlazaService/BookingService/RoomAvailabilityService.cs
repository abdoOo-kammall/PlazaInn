using PlazaCore.Entites;
using PlazaCore.RepositoryContract;
using PlazaCore.ServiceContract;
using PlazaCore.Specification.BookingSpecification;
using PlazaCore.Specification.RoomSpecification;
using Shared.DTO.Booking;
using Shared.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlazaService.BookingService
{
    public class RoomAvailabilityService : IRoomAvailabilityService
    {
        private readonly IGenericRepository<Booking> _bookingRepo;
        private readonly IGenericRepository<Room> _roomRepo;
        private readonly IGenericRepository<Hotel> _hotelRepo;

        public RoomAvailabilityService(
            IGenericRepository<Booking> bookingRepo,
            IGenericRepository<Room> roomRepo,
            IGenericRepository<Hotel> hotelRepo)
        {
            _bookingRepo = bookingRepo;
            _roomRepo = roomRepo;
            _hotelRepo = hotelRepo;
        }

        // ------------------------------------------------------
        // List rooms + availability
        // ------------------------------------------------------
        public async Task<IReadOnlyList<RoomAvailabilityDto>>
            GetHotelRoomsAvailabilityAsync(
                string encodedHotelId,
                RoomAvailabilityRequestDto request)
        {
            int hotelId = IdEncoder.DecodeId(encodedHotelId);

            var hotel = await _hotelRepo.GetByIdAsync(hotelId)
                ?? throw new Exception("Hotel not found");
            var spec = new RoomByHotelIdSpec(hotelId);
            var rooms = await _roomRepo.GetAllWithSpecAsync(spec);

            var bookingsSpec =
                new BookingInDateRangeSpec(hotelId, request.CheckIn, request.CheckOut);

            var bookings = await _bookingRepo.GetAllWithSpecAsync(bookingsSpec);

            int bookedRooms = bookings.Sum(b => b.RoomsBooked);
            int roomsLeftInHotel =
                hotel.NumOfAvailableRoomsToReserve - bookedRooms;

            var result = new List<RoomAvailabilityDto>();

            foreach (var room in rooms)
            {
                int people =
                    request.Adults +
                    request.Children +
                    (request.ExtraBed ? request.ExtraBedCount : 0);

                int capacity =
                    (room.MaxSleep * request.RoomsCount) +
                    (room.ExtraBed && request.ExtraBed
                        ? request.ExtraBedCount
                        : 0);

                bool isAvailable =
                    roomsLeftInHotel >= request.RoomsCount &&
                    capacity >= people;

                decimal estimatedPrice =
                    CalculateRoomEstimatedPrice(room, request);

                result.Add(new RoomAvailabilityDto
                {
                    EncodedRoomId = IdEncoder.EncodeId(room.Id),
                    Name = room.Name,
                    NameAr = room.NameAr,
                    PricePerNight = room.PricePerNight,
                    ExtraBedAvailable = room.ExtraBed,
                    PriceForExtraBed = room.PriceForExtraBed,
                    MaxSleep = room.MaxSleep,
                    IsAvailable = isAvailable,
                    RoomsLeftForHotelLimit = roomsLeftInHotel,
                    EstimatedTotalPriceForRequest = estimatedPrice
                });
            }

            return result;
        }

        // ------------------------------------------------------
        // Check availability for booking
        // ------------------------------------------------------
        public async Task<bool> CheckAvailabilityAsync(BookingRequestDto request)
        {
            int hotelId = IdEncoder.DecodeId(request.EncodedHotelId);
            int roomId = IdEncoder.DecodeId(request.EncodedRoomId);

            var hotel = await _hotelRepo.GetByIdAsync(hotelId)
                ?? throw new Exception("Hotel not found");

            var room = await _roomRepo.GetByIdAsync(roomId)
                ?? throw new Exception("Room not found");

            var bookingsSpec =
                new BookingInDateRangeSpec(hotelId, request.CheckIn, request.CheckOut);

            var bookings = await _bookingRepo.GetAllWithSpecAsync(bookingsSpec);

            int bookedRooms = bookings.Sum(b => b.RoomsBooked);

            if (bookedRooms + request.RoomsCount >
                hotel.NumOfAvailableRoomsToReserve)
                return false;

            int people =
                request.Adults +
                request.Children +
                (request.ExtraBed ? request.ExtraBedCount : 0);

            int capacity =
                (room.MaxSleep * request.RoomsCount) +
                (room.ExtraBed && request.ExtraBed
                    ? request.ExtraBedCount
                    : 0);

            return capacity >= people;
        }

        // ------------------------------------------------------
        // Calculate total price
        // ------------------------------------------------------
        public async Task<decimal> CalculateTotalPriceAsync(
            BookingRequestDto request)
        {
            int roomId = IdEncoder.DecodeId(request.EncodedRoomId);

            var room = await _roomRepo.GetByIdAsync(roomId)
                ?? throw new Exception("Room not found");

            int nights = (request.CheckOut - request.CheckIn).Days;
            if (nights <= 0) throw new Exception("Invalid dates");

            decimal basePrice =
                room.PricePerNight *
                nights *
                request.RoomsCount;

            decimal extraBedPrice = 0;
            if (request.ExtraBed && room.ExtraBed)
            {
                extraBedPrice =
                    room.PriceForExtraBed *
                    request.ExtraBedCount *
                    nights;
            }

            return basePrice + extraBedPrice;
        }

        private decimal CalculateRoomEstimatedPrice(
            Room room,
            RoomAvailabilityRequestDto request)
        {
            int nights = (request.CheckOut - request.CheckIn).Days;
            if (nights <= 0) return 0;

            decimal basePrice =
                room.PricePerNight *
                nights *
                request.RoomsCount;

            decimal extraBedPrice = 0;
            if (request.ExtraBed && room.ExtraBed)
            {
                extraBedPrice =
                    room.PriceForExtraBed *
                    request.ExtraBedCount *
                    nights;
            }

            return basePrice + extraBedPrice;
        }
    }

}
