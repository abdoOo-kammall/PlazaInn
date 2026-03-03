using AutoMapper;
using Plaza.Exceptions;
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
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace PlazaService.BookingService
{
    public class RoomAvailabilityService : IRoomAvailabilityService
    {
        private readonly IGenericRepository<Booking> _bookingRepo;
        private readonly IGenericRepository<Room> _roomRepo;
        private readonly IGenericRepository<Hotel> _hotelRepo;
        private readonly IMapper _mapper;

        public RoomAvailabilityService(
            IGenericRepository<Booking> bookingRepo,
            IGenericRepository<Room> roomRepo,
            IGenericRepository<Hotel> hotelRepo , IMapper mapper) 
           
        {
            _bookingRepo = bookingRepo;
            _roomRepo = roomRepo;
            _hotelRepo = hotelRepo;
            _mapper = mapper;
        }

        public async Task<IReadOnlyList<RoomAvailabilityDto>> GetHotelRoomsAvailabilityAsync(string encodedHotelId, RoomAvailabilityRequestDto request)
        {
            if (!IsDateRangeValid(request))
                throw new ArgumentException("Invalid date range");

            int hotelId = IdEncoder.DecodeId(encodedHotelId);

            var hotel = await _hotelRepo.GetByIdAsync(hotelId)
                ?? throw new NotFoundException("Hotel not found");

           
            var spec = new RoomByHotelIdSpec(hotelId);
            var rooms = await _roomRepo.GetAllWithSpecAsync(spec);

           
            var bookingsSpec = new BookingInDateRangeSpec(hotelId, request.CheckIn, request.CheckOut);
            var bookings = await _bookingRepo.GetAllWithSpecAsync(bookingsSpec);

            //int nights = (request.CheckOut - request.CheckIn).Days;

   
            var result = rooms.Select(room =>
            {
                bool isBooked = bookings.Any(b => b.RoomId == room.Id);

                return _mapper.Map<RoomAvailabilityDto>(room, opts =>
                {
                    //opts.Items["nights"] = nights;
                    opts.Items["isBooked"] = isBooked;
                });

            }).ToList();

            return result;
        }

        public async Task<bool> CheckAvailabilityAsync(BookingRequestDto request)
        {

            int roomId = IdEncoder.DecodeId(request.EncodedRoomId);

            var room = await _roomRepo.GetByIdAsync(roomId) ?? throw new Exception("Room not found");

            var bookingsSpec = new BookingInDateRangeSpec(room.HotelId, request.CheckIn, request.CheckOut);
            var bookings = await _bookingRepo.GetAllWithSpecAsync(bookingsSpec);

            bool isBooked = bookings.Any(b => b.RoomId == room.Id);
            return !isBooked;
        }

        public async Task<decimal> CalculateTotalPriceAsync(BookingRequestDto request)
        {
            int roomId = IdEncoder.DecodeId(request.EncodedRoomId);

            var room = await _roomRepo.GetByIdAsync(roomId) ?? throw new NotFoundException("Room not found");

            int nights = (request.CheckOut - request.CheckIn).Days;
            if (nights <= 0) throw new ArgumentException("Invalid dates");

            return room.PricePerNight * nights * 128 / 100;
        }

        public bool IsDateRangeValid(RoomAvailabilityRequestDto request)
        {
            return request.CheckOut > request.CheckIn;
        }
    }
}
