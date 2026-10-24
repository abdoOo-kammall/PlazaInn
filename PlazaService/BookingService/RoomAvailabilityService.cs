using AutoMapper;
using Microsoft.Extensions.Options;
using Plaza.Exceptions;
using PlazaCore.Entites;
using PlazaCore.RepositoryContract;
using PlazaCore.ServiceContract;
using PlazaCore.Specification.BookingSpecification;
using PlazaCore.Specification.RoomSpecification;
using Shared.DTO.Booking;
using Shared.Enums;
using Shared.Helper;
using Shared.Options;
using Shared.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlazaService.BookingService
{
    public class RoomAvailabilityService : IRoomAvailabilityService
    {
        private readonly IGenericRepository<Booking> _bookingRepo;
        private readonly IGenericRepository<Room> _roomRepo;
        private readonly IGenericRepository<Hotel> _hotelRepo;
        private readonly IMapper _mapper;
        private readonly BookingOptions _options;

        public RoomAvailabilityService(
            IGenericRepository<Booking> bookingRepo,
            IGenericRepository<Room> roomRepo,
            IGenericRepository<Hotel> hotelRepo,
            IMapper mapper,
            IOptions<BookingOptions> options)
        {
            _bookingRepo = bookingRepo;
            _roomRepo = roomRepo;
            _hotelRepo = hotelRepo;
            _mapper = mapper;
            _options = options.Value;
        }

        private DateTime PendingCutoff => DateTime.UtcNow.AddMinutes(-_options.PendingExpiryMinutes);

        public async Task<IReadOnlyList<RoomAvailabilityDto>> GetHotelRoomsAvailabilityAsync(string encodedHotelId, RoomAvailabilityRequestDto request)
        {
            var stayError = StayRules.GetStayLengthError(request.CheckIn, request.CheckOut);
            if (stayError != null)
                throw new ArgumentException(stayError);

            int hotelId = IdEncoder.DecodeId(encodedHotelId);

            _ = await _hotelRepo.GetByIdAsync(hotelId)
                ?? throw new NotFoundException("Hotel not found");

            var rooms = await _roomRepo.GetAllWithSpecAsync(new RoomByHotelIdSpec(hotelId));

            var bookings = (await _bookingRepo.GetAllWithSpecAsync(
                new BookingInDateRangeSpec(hotelId, request.CheckIn, request.CheckOut, PendingCutoff))).ToList();

            return rooms.Select(room =>
            {
                var availableRooms = CalculateAvailableRooms(room, room.Hotel, bookings, request.CheckIn, request.CheckOut);

                return _mapper.Map<RoomAvailabilityDto>(room, opts =>
                {
                    opts.Items["availableRooms"] = availableRooms;
                    opts.Items["roomsCount"] = request.RoomsCount;
                });
            }).ToList();
        }

        public async Task<bool> CheckAvailabilityAsync(BookingQuoteRequestDto request)
        {
            var room = await GetRoomWithHotelAsync(IdEncoder.DecodeId(request.EncodedRoomId));
            ValidateStay(room, request);

            var available = await GetAvailableRoomsAsync(room, request.CheckIn, request.CheckOut);
            return available >= request.RoomsCount;
        }

        public async Task<int> GetAvailableRoomsAsync(Room room, DateTime checkIn, DateTime checkOut, int? excludeBookingId = null)
        {
            var hotel = room.Hotel ?? await _hotelRepo.GetByIdAsync(room.HotelId)
                ?? throw new NotFoundException("Hotel not found");

            var bookings = await _bookingRepo.GetAllWithSpecAsync(
                new BookingInDateRangeSpec(room.HotelId, checkIn, checkOut, PendingCutoff, excludeBookingId));

            return CalculateAvailableRooms(room, hotel, bookings, checkIn, checkOut);
        }

        public async Task<decimal> CalculateTotalPriceAsync(BookingQuoteRequestDto request)
        {
            var room = await GetRoomWithHotelAsync(IdEncoder.DecodeId(request.EncodedRoomId));
            ValidateStay(room, request);

            return CalculateTotalPrice(room, request.RoomsCount, request.ExtraBed, request.CheckIn, request.CheckOut);
        }

        public decimal CalculateTotalPrice(Room room, int roomsCount, bool extraBed, DateTime checkIn, DateTime checkOut)
        {
            var stayError = StayRules.GetStayLengthError(checkIn, checkOut);
            if (stayError != null)
                throw new ArgumentException(stayError);
            if (roomsCount < 1)
                throw new ArgumentException("At least one room must be booked.");
            if (extraBed && !room.ExtraBed)
                throw new ArgumentException("Extra bed is not available for this room.");

            var nights = StayRules.GetNights(checkIn, checkOut);
            var nightlyPrice = (room.PricePerNight * roomsCount) + (extraBed ? room.PriceForExtraBed : 0m);
            var total = nightlyPrice * nights * (1 + _options.TaxRate);

            return Math.Round(total, 2, MidpointRounding.AwayFromZero);
        }

        public void ValidateStay(Room room, BookingQuoteRequestDto request)
        {
            if (request.CheckIn.Date < DateTime.UtcNow.Date)
                throw new ArgumentException("Check-in date cannot be in the past.");
            var stayError = StayRules.GetStayLengthError(request.CheckIn, request.CheckOut);
            if (stayError != null)
                throw new ArgumentException(stayError);
            if (request.ExtraBed && !room.ExtraBed)
                throw new ArgumentException("Extra bed is not available for this room.");

            if (room.MaxSleep > 0)
            {
                var capacity = room.MaxSleep * request.RoomsCount + (request.ExtraBed ? 1 : 0);
                if (request.Adults + request.Children > capacity)
                    throw new ArgumentException($"The selected rooms can accommodate at most {capacity} guests.");
            }
        }

        public bool IsDateRangeValid(RoomAvailabilityRequestDto request)
        {
            return StayRules.GetStayLengthError(request.CheckIn, request.CheckOut) == null;
        }

        private async Task<Room> GetRoomWithHotelAsync(int roomId)
        {
            return await _roomRepo.GetByIdWithSpecAsync(new RoomWithHotelSpec(roomId))
                ?? throw new NotFoundException("Room not found");
        }

        private static bool IsSuite(Room room) => room.Type == RoomType.Suite;

        private static int GetPoolCapacity(Hotel hotel, Room room) =>
            IsSuite(room) ? hotel.NumOfAvailableSuitesToReserve : hotel.NumOfAvailableRoomsToReserve;

        private static int CalculateAvailableRooms(Room room, Hotel hotel, IEnumerable<Booking> overlappingBookings, DateTime checkIn, DateTime checkOut)
        {
            var capacity = GetPoolCapacity(hotel, room);
            if (capacity <= 0)
                return 0;

            var samePool = overlappingBookings
                .Where(b => b.Room != null && IsSuite(b.Room) == IsSuite(room))
                .ToList();

            // Peak occupancy across the requested nights; legacy rows stored RoomsBooked = 0, which still occupies one room.
            var peakBooked = 0;
            for (var night = checkIn.Date; night < checkOut.Date; night = night.AddDays(1))
            {
                var bookedThatNight = samePool
                    .Where(b => b.CheckIn.Date <= night && b.CheckOut.Date > night)
                    .Sum(b => Math.Max(1, b.RoomsBooked));

                peakBooked = Math.Max(peakBooked, bookedThatNight);
            }

            return Math.Max(0, capacity - peakBooked);
        }
    }
}
