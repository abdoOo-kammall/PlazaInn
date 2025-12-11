//using AutoMapper;
//using PlazaCore.Entites;
//using PlazaCore.RepositoryContract;
//using PlazaCore.ServiceContract;
//using PlazaCore.Specification.BookingSpecification;
//using PlazaCore.Specification.RoomSpecification;
//using Shared.DTO.Booking;
//using Shared.Enums;
//using Shared.Security;
//using System;
//using System.Linq;
//using System.Threading.Tasks;

//namespace PlazaService.BookingService
//{
//    public class BookingService : IBookingService
//    {
//        private readonly IGenericRepository<Booking> _bookingRepo;
//        private readonly IGenericRepository<Room> _roomRepo;
//        private readonly IMapper _mapper;

//        public BookingService(
//            IGenericRepository<Booking> bookingRepo,
//            IGenericRepository<Room> roomRepo,
//            IMapper mapper)
//        {
//            _bookingRepo = bookingRepo;
//            _roomRepo = roomRepo;
//            _mapper = mapper;
//        }

//        public async Task<bool> CheckAvailabilityAsync(BookingRequestDto request)
//        {
//            int roomId = IdEncoder.DecodeId(request.EncodedHotelId);
//            var spec01 = new RoomByHotelIdSpec(roomId);
//            var room = await _roomRepo.GetByIdWithSpecAsync(spec01);
//            if (room == null) throw new Exception("Room not found");

//            int nights = (request.CheckOut - request.CheckIn).Days;
//            if (nights <= 0) throw new Exception("Invalid dates");

//            int requiredRooms = request.Adults; // كل شخص غرفة

//            var spec02 = new BookingInDateRangeSpec(room.Id, request.CheckIn, request.CheckOut);
//            var bookings = await _bookingRepo.GetAllWithSpecAsync(spec02);

//            int booked = bookings.Sum(b => b.RoomsBooked);
//            int maxRooms = room.Hotel.NumOfAvailableRoomsToReserve;

//            return booked + requiredRooms <= maxRooms;
//        }

//        public async Task<BookingResponseDto> CreateBookingAsync(BookingRequestDto request)
//        {
//            var booking = _mapper.Map<Booking>(request);

//            int roomId = IdEncoder.DecodeId(request.EncodedRoomId);
//            var room = await _roomRepo.GetByIdAsync(roomId);
//            if (room == null) throw new Exception("Room not found");

//            booking.RoomId = roomId;
//            booking.RoomsBooked = request.Adults;
//            booking.TotalPrice = await CalculateTotalPriceAsync(request);
//            booking.Status = BookingStatus.Pending;

//            await _bookingRepo.AddAsync(booking);
//            await _bookingRepo.SaveChangesAsync();

//            return _mapper.Map<BookingResponseDto>(booking);
//        }


//        public async Task ConfirmBookingPaymentAsync(string paymentReference)
//        {
//            var spec = new BookingByPaymentRefSpec(paymentReference);
//            var booking = (await _bookingRepo.GetAllWithSpecAsync(spec)).FirstOrDefault();

//            if (booking == null)
//                throw new Exception("Invalid payment reference");

//            booking.IsPaid = true;
//            booking.Status = BookingStatus.Confirmed;

//            await _bookingRepo.SaveChangesAsync();
//        }

//        public async Task<BookingResponseDto?> GetBookingByIdAsync(int id)
//        {
//            var booking = await _bookingRepo.GetByIdAsync(id);
//            if (booking == null) return null;

//            return _mapper.Map<BookingResponseDto>(booking);
//        }

//        public async Task<decimal> CalculateTotalPriceAsync(BookingRequestDto request)
//        {
//            int roomId = IdEncoder.DecodeId(request.EncodedRoomId);

//            var room = await _roomRepo.GetByIdAsync(roomId);
//            if (room == null)
//                throw new Exception("Room not found");

//            int nights = (request.CheckOut - request.CheckIn).Days;
//            if (nights <= 0)
//                throw new Exception("Invalid dates");

//            int roomsCount = request.Adults; // كل شخص غرفة

//            decimal totalPrice = room.PricePerNight * nights * roomsCount;

//            return totalPrice;
//        }

//    }
//}
using AutoMapper;
using PlazaCore.Entites;
using PlazaCore.RepositoryContract;
using PlazaCore.ServiceContract;
using PlazaCore.Specification.BookingSpecification;
using PlazaCore.Specification.RoomSpecification;
using Shared.DTO.Booking;
using Shared.Enums;
using Shared.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class BookingService : IBookingService
{
    private readonly IGenericRepository<Booking> _bookingRepo;
    private readonly IGenericRepository<Room> _roomRepo;
    private readonly IGenericRepository<Hotel> _hotelRepo;
    private readonly IMapper _mapper;

    public BookingService(
        IGenericRepository<Booking> bookingRepo,
        IGenericRepository<Room> roomRepo,
        IGenericRepository<Hotel> hotelRepo,
        IMapper mapper)
    {
        _bookingRepo = bookingRepo;
        _roomRepo = roomRepo;
        _hotelRepo = hotelRepo;
        _mapper = mapper;
    }

    /// <summary>
    /// Check availability for a specific request (room + hotel + dates + roomsCount + extra beds)
    /// Returns true if request can be accepted according to hotel-wide limit and room capacity.
    /// </summary>
    public async Task<bool> CheckAvailabilityAsync(BookingRequestDto request)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        int hotelId = IdEncoder.DecodeId(request.EncodedHotelId);
        int roomId = IdEncoder.DecodeId(request.EncodedRoomId);

        var hotel = await _hotelRepo.GetByIdAsync(hotelId);
        if (hotel == null) throw new InvalidOperationException("Hotel not found");

        var room = await _roomRepo.GetByIdAsync(roomId);
        if (room == null) throw new InvalidOperationException("Room not found");

        int nights = (request.CheckOut - request.CheckIn).Days;
        if (nights <= 0) throw new ArgumentException("Invalid check-in/check-out dates");

        int requestedRooms = request.RoomsCount > 0 ? request.RoomsCount : 1;

        // total people to accommodate
        int totalPeople = request.Adults + request.Children + (request.ExtraBed ? request.ExtraBedCount : 0);

        // per-room capacity check (considering that ExtraBed gives additional capacity)
        int baseCapacity = room.MaxSleep * requestedRooms;
        int extraCapacity = 0;
        if (room.ExtraBed && request.ExtraBed)
        {
            // assume ExtraBedCount is distributed across requestedRooms; we simply add requested extra beds
            extraCapacity = request.ExtraBedCount;
        }
        if (totalPeople > baseCapacity + extraCapacity)
            return false; // can't fit people in chosen rooms

        // get hotel-level bookings in same date range
        var hotelBookingsSpec = new BookingInDateRangeSpec(hotelId, request.CheckIn, request.CheckOut);
        var hotelBookings = await _bookingRepo.GetAllWithSpecAsync(hotelBookingsSpec);

        int alreadyBookedTotal = hotelBookings.Sum(b => b.RoomsBooked);

        // check hotel-level admin limit
        if ((alreadyBookedTotal + requestedRooms) > hotel.NumOfAvailableRoomsToReserve)
            return false;

        // optional: also check per-room quantity if you have Room.Quantity property (not required)
        // if (room.Quantity > 0) { var bookedForThisRoom = hotelBookings.Where(b => b.RoomId == roomId).Sum(b => b.RoomsBooked); if (bookedForThisRoom + requestedRooms > room.Quantity) return false; }

        return true;
    }

    /// <summary>
    /// Create booking (re-check availability before saving). Price calculated by CalculateTotalPriceAsync.
    /// </summary>
    public async Task<BookingResponseDto> CreateBookingAsync(BookingRequestDto request)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        int hotelId = IdEncoder.DecodeId(request.EncodedHotelId);
        int roomId = IdEncoder.DecodeId(request.EncodedRoomId);

        var hotel = await _hotelRepo.GetByIdAsync(hotelId) ?? throw new InvalidOperationException("Hotel not found");
        var room = await _roomRepo.GetByIdAsync(roomId) ?? throw new InvalidOperationException("Room not found");

        int nights = (request.CheckOut - request.CheckIn).Days;
        if (nights <= 0) throw new ArgumentException("Invalid dates");

        int requestedRooms = request.RoomsCount > 0 ? request.RoomsCount : 1;

        // Re-check availability to reduce race conditions
        var hotelBookingsSpec = new BookingInDateRangeSpec(hotelId, request.CheckIn, request.CheckOut);
        var hotelBookings = await _bookingRepo.GetAllWithSpecAsync(hotelBookingsSpec);
        int alreadyBookedTotal = hotelBookings.Sum(b => b.RoomsBooked);
        if ((alreadyBookedTotal + requestedRooms) > hotel.NumOfAvailableRoomsToReserve)
            throw new InvalidOperationException("Not enough rooms available for the selected dates");

        // Map DTO -> Booking using AutoMapper (hotelId, roomId and RoomsBooked are mapped by profile)
        var booking = _mapper.Map<Booking>(request);

        // calculate price and set other fields
        booking.TotalPrice = await CalculateTotalPriceAsync(request);
        booking.IsPaid = false;
        booking.Status = BookingStatus.Pending;

        await _bookingRepo.AddAsync(booking);
        await _bookingRepo.SaveChangesAsync();

        return _mapper.Map<BookingResponseDto>(booking);
    }

    /// <summary>
    /// Confirm payment by paymentReference
    /// </summary>
    public async Task ConfirmBookingPaymentAsync(string paymentReference)
    {
        if (string.IsNullOrWhiteSpace(paymentReference)) throw new ArgumentNullException(nameof(paymentReference));

        var spec = new BookingByPaymentRefSpec(paymentReference);
        var bookings = await _bookingRepo.GetAllWithSpecAsync(spec);
        var booking = bookings.FirstOrDefault();

        if (booking == null) throw new InvalidOperationException("Invalid payment reference");

        booking.IsPaid = true;
        booking.Status = BookingStatus.Confirmed;

        await _bookingRepo.SaveChangesAsync();
    }

    public async Task<BookingResponseDto?> GetBookingByIdAsync(int id)
    {
        var booking = await _bookingRepo.GetByIdAsync(id);
        if (booking == null) return null;
        return _mapper.Map<BookingResponseDto>(booking);
    }

    /// <summary>
    /// Calculates booking total: base price + extra beds (per-night).
    /// </summary>
    public async Task<decimal> CalculateTotalPriceAsync(BookingRequestDto request)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        int roomId = IdEncoder.DecodeId(request.EncodedRoomId);
        var room = await _roomRepo.GetByIdAsync(roomId) ?? throw new InvalidOperationException("Room not found");

        int nights = (request.CheckOut - request.CheckIn).Days;
        if (nights <= 0) throw new ArgumentException("Invalid dates");

        int roomsCount = request.RoomsCount > 0 ? request.RoomsCount : 1;

        decimal basePrice = room.PricePerNight * nights * roomsCount;

        decimal extraBedCost = 0;
        if (request.ExtraBed && room.ExtraBed)
        {
            extraBedCost = room.PriceForExtraBed * request.ExtraBedCount * nights;
        }

        return basePrice + extraBedCost;
    }
}
