using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Plaza.Exceptions;
using PlazaCore.Entites;
using PlazaCore.RepositoryContract;
using PlazaCore.ServiceContract;
using PlazaCore.Specification.BookingSpecification;
using PlazaCore.Specification.RoomSpecification;
using Shared.DTO.Booking;
using Shared.Enums;
using Shared.Options;
using Shared.Security;
using System.Collections.Generic;
using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace PlazaService.BookingService
{
    public class BookingService : IBookingService
    {
        private readonly IGenericRepository<Booking> _bookingRepo;
        private readonly IGenericRepository<Room> _roomRepo;
        private readonly IRoomAvailabilityService _availabilityService;
        private readonly IMapper _mapper;
        private readonly BookingOptions _options;
        private readonly ILogger<BookingService> _logger;

        public BookingService(
            IGenericRepository<Booking> bookingRepo,
            IGenericRepository<Room> roomRepo,
            IRoomAvailabilityService availabilityService,
            IMapper mapper,
            IOptions<BookingOptions> options,
            ILogger<BookingService> logger)
        {
            _bookingRepo = bookingRepo;
            _roomRepo = roomRepo;
            _availabilityService = availabilityService;
            _mapper = mapper;
            _options = options.Value;
            _logger = logger;
        }

        private const string MerchantOrderPrefix = "PLZ";

        private DateTime PendingCutoff => DateTime.UtcNow.AddMinutes(-_options.PendingExpiryMinutes);

        public async Task<BookingResponseDto> CreateBookingAsync(BookingRequestDto request, string userId, string? userEmail)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("An authenticated user is required to create a booking.", nameof(userId));

            var hotelId = IdEncoder.DecodeId(request.EncodedHotelId);
            var roomId = IdEncoder.DecodeId(request.EncodedRoomId);

            // Serializable keeps the availability read and the insert atomic, so concurrent requests cannot overbook.
            var booking = await _bookingRepo.ExecuteInTransactionAsync(async () =>
            {
                var room = await _roomRepo.GetByIdWithSpecAsync(new RoomWithHotelSpec(roomId))
                    ?? throw new NotFoundException("Room not found");

                if (room.HotelId != hotelId)
                    throw new ArgumentException("The selected room does not belong to the selected hotel.");

                _availabilityService.ValidateStay(room, request);

                var totalPrice = _availabilityService.CalculateTotalPrice(
                    room, request.RoomsCount, request.ExtraBed, request.CheckIn, request.CheckOut);

                var available = await _availabilityService.GetAvailableRoomsAsync(room, request.CheckIn.Date, request.CheckOut.Date);
                if (available < request.RoomsCount)
                    throw new ConflictException(available == 0
                        ? "No rooms of this type are available for the selected dates."
                        : $"Only {available} room(s) of this type are available for the selected dates.");

                var entity = _mapper.Map<Booking>(request);
                entity.HotelId = room.HotelId;
                entity.RoomId = room.Id;
                entity.CheckIn = request.CheckIn.Date;
                entity.CheckOut = request.CheckOut.Date;
                entity.TotalPrice = totalPrice;
                entity.Status = BookingStatus.Pending;
                entity.IsPaid = false;
                entity.MerchantOrderId = $"{MerchantOrderPrefix}-{Guid.NewGuid():N}";
                entity.UserId = userId;
                if (!string.IsNullOrWhiteSpace(userEmail))
                    entity.CustomerEmail = userEmail;

                await _bookingRepo.AddAsync(entity);
                await _bookingRepo.SaveChangesAsync();
                return entity;
            }, IsolationLevel.Serializable);

            return _mapper.Map<BookingResponseDto>(booking);
        }

        public async Task<BookingResponseDto> GetBookingByIdAsync(int bookingId, string userId, bool isAdmin)
        {
            var booking = await GetBookingOrThrowAsync(bookingId);
            EnsureCanAccess(booking, userId, isAdmin);
            return _mapper.Map<BookingResponseDto>(booking);
        }

        public async Task<BookingResponseDto> CancelBookingAsync(int bookingId, string userId, bool isAdmin)
        {
            var booking = await _bookingRepo.ExecuteInTransactionAsync(async () =>
            {
                var entity = await GetBookingOrThrowAsync(bookingId);
                EnsureCanAccess(entity, userId, isAdmin);

                if (entity.Status == BookingStatus.Cancelled)
                    return entity;

                if (entity.Status == BookingStatus.Confirmed && !isAdmin)
                    throw new ConflictException("Paid bookings can only be cancelled by an administrator.");

                entity.Status = BookingStatus.Cancelled;
                entity.UpdatedAt = DateTime.UtcNow;
                await _bookingRepo.SaveChangesAsync();
                return entity;
            }, IsolationLevel.RepeatableRead);

            return _mapper.Map<BookingResponseDto>(booking);
        }

        public async Task<BookingPaymentInfo> PrepareBookingForPaymentAsync(int bookingId, string userId, bool isAdmin)
        {
            var booking = await GetBookingOrThrowAsync(bookingId);
            EnsureCanAccess(booking, userId, isAdmin);

            if (booking.Status != BookingStatus.Pending)
                throw new ConflictException($"Only pending bookings can be paid (current status: {booking.Status}).");

            if (booking.CreatedAt < PendingCutoff)
                throw new ConflictException("The booking hold has expired. Please create a new booking.");

            return new BookingPaymentInfo
            {
                BookingId = booking.Id,
                AmountCents = ToAmountCents(booking.TotalPrice),
                // Paymob rejects reused merchant order ids, so each payment attempt gets a fresh one.
                // The embedded booking id lets a payment on an older attempt still be matched to its booking.
                MerchantOrderId = $"{MerchantOrderPrefix}-{booking.Id}-{Guid.NewGuid():N}",
                CustomerName = booking.CustomerName,
                CustomerEmail = booking.CustomerEmail,
                CustomerPhone = booking.CustomerPhone
            };
        }

        public async Task AttachPaymentReferenceAsync(int bookingId, string paymentReference, string merchantOrderId)
        {
            var booking = await _bookingRepo.GetByIdAsync(bookingId) ??
                throw new NotFoundException("Booking not found");

            booking.PaymentReference = paymentReference;
            booking.MerchantOrderId = merchantOrderId;
            booking.UpdatedAt = DateTime.UtcNow;

            await _bookingRepo.SaveChangesAsync();
        }

        public async Task EnsureCanAccessPaymentAsync(string paymentReference, string? merchantOrderId, string userId, bool isAdmin)
        {
            var (booking, _) = await FindBookingForPaymentAsync(paymentReference, merchantOrderId);
            if (booking == null)
                throw new NotFoundException("No booking found for this payment.");

            EnsureCanAccess(booking, userId, isAdmin);
        }

        public async Task<bool> ConfirmBookingPaymentAsync(long paymobOrderId, long paidAmountCents, string? merchantOrderId)
        {
            var paymentReference = paymobOrderId.ToString();

            return await _bookingRepo.ExecuteInTransactionAsync(async () =>
            {
                var (booking, matchedByReference) = await FindBookingForPaymentAsync(paymentReference, merchantOrderId);
                if (booking == null)
                {
                    _logger.LogCritical(
                        "Paid Paymob order {PaymobOrderId} (merchant order {MerchantOrderId}) does not match any booking. Manual reconciliation or refund required.",
                        paymobOrderId, merchantOrderId);
                    return false;
                }

                if (booking.Status == BookingStatus.Confirmed)
                {
                    if (!string.Equals(booking.PaymentReference, paymentReference, StringComparison.Ordinal))
                    {
                        _logger.LogCritical(
                            "Booking {BookingId} is already confirmed by Paymob order {ConfirmedOrderId} but was paid again by order {PaymobOrderId}. Manual refund required.",
                            booking.Id, booking.PaymentReference, paymobOrderId);
                    }
                    return true;
                }

                var expectedCents = ToAmountCents(booking.TotalPrice);
                if (paidAmountCents != expectedCents)
                {
                    _logger.LogError(
                        "Amount mismatch for booking {BookingId} / Paymob order {PaymobOrderId}: paid {Paid} cents, expected {Expected} cents.",
                        booking.Id, paymobOrderId, paidAmountCents, expectedCents);
                    return false;
                }

                // A fallback match was found through the booking id embedded in the merchant order id, so the stored
                // (newer) merchant order id is expected to differ.
                if (matchedByReference && booking.MerchantOrderId != null && !string.Equals(booking.MerchantOrderId, merchantOrderId, StringComparison.Ordinal))
                {
                    _logger.LogError(
                        "Merchant order id mismatch for booking {BookingId} / Paymob order {PaymobOrderId}: received {Received}, expected {Expected}.",
                        booking.Id, paymobOrderId, merchantOrderId, booking.MerchantOrderId);
                    return false;
                }

                // Expired pending holds no longer count against inventory, so the rooms may have been sold in the meantime.
                var holdLost = booking.Status == BookingStatus.Cancelled
                    || (booking.Status == BookingStatus.Pending && booking.CreatedAt < PendingCutoff);

                if (holdLost)
                {
                    var available = await _availabilityService.GetAvailableRoomsAsync(
                        booking.Room, booking.CheckIn, booking.CheckOut, excludeBookingId: booking.Id);

                    if (available < Math.Max(1, booking.RoomsBooked))
                    {
                        _logger.LogCritical(
                            "Booking {BookingId} ({Status}) was paid late (Paymob order {PaymobOrderId}) and the rooms are no longer available. Manual refund required.",
                            booking.Id, booking.Status, paymobOrderId);
                        return false;
                    }

                    _logger.LogWarning("Confirming {Status} booking {BookingId} after late payment (Paymob order {PaymobOrderId}).",
                        booking.Status, booking.Id, paymobOrderId);
                }

                if (!matchedByReference)
                {
                    _logger.LogWarning(
                        "Booking {BookingId} matched Paymob order {PaymobOrderId} via merchant order id {MerchantOrderId} (payment reference had been replaced by {PreviousReference}).",
                        booking.Id, paymobOrderId, merchantOrderId, booking.PaymentReference);
                }

                booking.PaymentReference = paymentReference;
                booking.MerchantOrderId = merchantOrderId ?? booking.MerchantOrderId;
                booking.IsPaid = true;
                booking.Status = BookingStatus.Confirmed;
                booking.UpdatedAt = DateTime.UtcNow;

                await _bookingRepo.SaveChangesAsync();
                return true;
            }, IsolationLevel.Serializable);
        }

        public async Task<int> CancelExpiredPendingBookingsAsync()
        {
            var cutoff = PendingCutoff;

            return await _bookingRepo.ExecuteInTransactionAsync(async () =>
            {
                var expired = (await _bookingRepo.GetAllWithSpecAsync(new ExpiredPendingBookingsSpec(cutoff))).ToList();
                if (expired.Count == 0)
                    return 0;

                var now = DateTime.UtcNow;
                foreach (var booking in expired)
                {
                    booking.Status = BookingStatus.Cancelled;
                    booking.UpdatedAt = now;
                }

                await _bookingRepo.SaveChangesAsync();
                return expired.Count;
            }, IsolationLevel.RepeatableRead);
        }

        public async Task<IReadOnlyList<BookingResponseDto>> GetUserBookingsAsync(
            string userId,
            int? hotelId = null,
            DateTime? from = null,
            DateTime? to = null,
            BookingStatus? status = null)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User id is required.", nameof(userId));

            var bookings = await _bookingRepo.GetAllWithSpecAsync(new BookingFilterSpec(userId, hotelId, from, to, status));
            return _mapper.Map<IReadOnlyList<BookingResponseDto>>(bookings);
        }

        public async Task<IReadOnlyList<BookingResponseDto>> GetAdminBookingsAsync(
            int? hotelId = null,
            DateTime? from = null,
            DateTime? to = null,
            BookingStatus? status = null)
        {
            var bookings = await _bookingRepo.GetAllWithSpecAsync(new BookingFilterSpec(null, hotelId, from, to, status));
            return _mapper.Map<IReadOnlyList<BookingResponseDto>>(bookings);
        }

        private async Task<Booking> GetBookingOrThrowAsync(int bookingId)
        {
            return await _bookingRepo.GetByIdWithSpecAsync(new BookingWithRoomAndHotelSpec(bookingId))
                ?? throw new NotFoundException("Booking not found");
        }

        /// <summary>
        /// Looks the booking up by the latest Paymob order id first, then by the booking id embedded in the
        /// Paymob-verified merchant order id (covers payments made on an older, overwritten payment attempt).
        /// </summary>
        private async Task<(Booking? Booking, bool MatchedByReference)> FindBookingForPaymentAsync(string paymentReference, string? merchantOrderId)
        {
            var booking = await _bookingRepo.GetByIdWithSpecAsync(new BookingByPaymentRefSpec(paymentReference));
            if (booking != null)
                return (booking, true);

            if (TryGetBookingIdFromMerchantOrderId(merchantOrderId, out var bookingId))
            {
                booking = await _bookingRepo.GetByIdWithSpecAsync(new BookingWithRoomAndHotelSpec(bookingId));
                if (booking != null)
                    return (booking, false);
            }

            return (null, false);
        }

        /// <summary>Parses merchant order ids of the form "PLZ-{bookingId}-{guid:N}".</summary>
        private static bool TryGetBookingIdFromMerchantOrderId(string? merchantOrderId, out int bookingId)
        {
            bookingId = 0;
            if (string.IsNullOrWhiteSpace(merchantOrderId))
                return false;

            var parts = merchantOrderId.Split('-');
            return parts.Length == 3
                && string.Equals(parts[0], MerchantOrderPrefix, StringComparison.Ordinal)
                && int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out bookingId)
                && bookingId > 0
                && Guid.TryParseExact(parts[2], "N", out _);
        }

        private static void EnsureCanAccess(Booking booking, string userId, bool isAdmin)
        {
            if (isAdmin)
                return;

            if (booking.UserId == null || !string.Equals(booking.UserId, userId, StringComparison.Ordinal))
                throw new ForbiddenException("You do not have access to this booking.");
        }

        private static long ToAmountCents(decimal amount) =>
            (long)Math.Round(amount * 100, MidpointRounding.AwayFromZero);
    }
}
