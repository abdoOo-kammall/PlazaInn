using AutoMapper;
using Plaza.Exceptions;
using PlazaCore.Entites;
using PlazaCore.RepositoryContract;
using PlazaCore.ServiceContract;
using PlazaCore.Specification.BookingSpecification;
using Shared.DTO.Booking;
using Shared.Enums;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PlazaService.BookingService
{
    public class BookingService : IBookingService
    {
        private readonly IGenericRepository<Booking> _bookingRepo;
        private readonly IRoomAvailabilityService _availabilityService;
        private readonly IMapper _mapper;

        public BookingService(
            IGenericRepository<Booking> bookingRepo,
            IRoomAvailabilityService availabilityService,
            IMapper mapper)
        {
            _bookingRepo = bookingRepo;
            _availabilityService = availabilityService;
            _mapper = mapper;
        }

        public async Task<BookingResponseDto> CreateBookingAsync(BookingRequestDto request)
        {
            //bool available = await _availabilityService.CheckAvailabilityAsync(request);
            //if (!available)
            //    throw new Exception("Room not available");

            var booking = _mapper.Map<Booking>(request);
            booking.TotalPrice = await _availabilityService.CalculateTotalPriceAsync(request);
            booking.Status = BookingStatus.Pending;
            booking.IsPaid = false;

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

        public async Task<BookingResponseDto?> GetBookingByIdAsync(int bookingId)
        {
            var booking = await _bookingRepo.GetByIdAsync(bookingId);
            return booking == null ? null : _mapper.Map<BookingResponseDto>(booking);
        }

        public async Task AttachPaymentReferenceAsync(int bookingId, string paymentReference)
        {
            var booking = await _bookingRepo.GetByIdAsync(bookingId) ??
                throw new NotFoundException("Booking not found");

            booking.PaymentReference = paymentReference;
            await _bookingRepo.SaveChangesAsync();
        }

    }
}
