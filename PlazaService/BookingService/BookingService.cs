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
using System.Linq;
using System.Threading.Tasks;

namespace PlazaService.BookingService
{
    public class BookingService : IBookingService
    {
        private readonly IGenericRepository<Booking> _bookingRepo;
        private readonly IGenericRepository<Room> _roomRepo;
        private readonly IMapper _mapper;

        public BookingService(
            IGenericRepository<Booking> bookingRepo,
            IGenericRepository<Room> roomRepo,
            IMapper mapper)
        {
            _bookingRepo = bookingRepo;
            _roomRepo = roomRepo;
            _mapper = mapper;
        }

        public async Task<bool> CheckAvailabilityAsync(BookingRequestDto request)
        {
            int roomId = IdEncoder.DecodeId(request.EncodedHotelId);
            var spec01 = new RoomByHotelIdSpec(roomId);
            var room = await _roomRepo.GetByIdWithSpecAsync(spec01);
            if (room == null) throw new Exception("Room not found");

            int nights = (request.CheckOut - request.CheckIn).Days;
            if (nights <= 0) throw new Exception("Invalid dates");

            int requiredRooms = request.Adults; // كل شخص غرفة

            var spec02 = new BookingInDateRangeSpec(room.Id, request.CheckIn, request.CheckOut);
            var bookings = await _bookingRepo.GetAllWithSpecAsync(spec02);

            int booked = bookings.Sum(b => b.RoomsBooked);
            int maxRooms = room.Hotel.NumOfAvailableRoomsToReserve;

            return booked + requiredRooms <= maxRooms;
        }

        public async Task<BookingResponseDto> CreateBookingAsync(BookingRequestDto request)
        {
            var booking = _mapper.Map<Booking>(request);

            int roomId = IdEncoder.DecodeId(request.EncodedRoomId);
            var room = await _roomRepo.GetByIdAsync(roomId);
            if (room == null) throw new Exception("Room not found");

            booking.RoomId = roomId;
            booking.RoomsBooked = request.Adults;
            booking.TotalPrice = await CalculateTotalPriceAsync(request);
            booking.Status = BookingStatus.Pending;

            await _bookingRepo.AddAsync(booking);
            await _bookingRepo.SaveChangesAsync();

            return _mapper.Map<BookingResponseDto>(booking);
        }


        public async Task ConfirmBookingPaymentAsync(string paymentReference)
        {
            var spec = new BookingByPaymentRefSpec(paymentReference);
            var booking = (await _bookingRepo.GetAllWithSpecAsync(spec)).FirstOrDefault();

            if (booking == null)
                throw new Exception("Invalid payment reference");

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

        public async Task<decimal> CalculateTotalPriceAsync(BookingRequestDto request)
        {
            int roomId = IdEncoder.DecodeId(request.EncodedRoomId);

            var room = await _roomRepo.GetByIdAsync(roomId);
            if (room == null)
                throw new Exception("Room not found");

            int nights = (request.CheckOut - request.CheckIn).Days;
            if (nights <= 0)
                throw new Exception("Invalid dates");

            int roomsCount = request.Adults; // كل شخص غرفة

            decimal totalPrice = room.PricePerNight * nights * roomsCount;

            return totalPrice;
        }

    }
}
