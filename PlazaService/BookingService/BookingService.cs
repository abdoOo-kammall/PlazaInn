using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Plaza.Exceptions;
using PlazaCore.Entites;
using PlazaCore.RepositoryContract;
using PlazaCore.ServiceContract;
using PlazaCore.Specification.BookingSpecification;
using PlazaCore.Specification.PackageSpecification;
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
        private readonly IGenericRepository<Trip> _tripRepo;
        private readonly IGenericRepository<Transport> _transportRepo;
        private readonly IGenericRepository<Event> _eventRepo;
        private readonly IGenericRepository<Package> _packageRepo;
        private readonly IRoomAvailabilityService _availabilityService;
        private readonly IMapper _mapper;
        private readonly BookingOptions _options;
        private readonly ILogger<BookingService> _logger;

        public BookingService(
            IGenericRepository<Booking> bookingRepo,
            IGenericRepository<Room> roomRepo,
            IGenericRepository<Trip> tripRepo,
            IGenericRepository<Transport> transportRepo,
            IGenericRepository<Event> eventRepo,
            IGenericRepository<Package> packageRepo,
            IRoomAvailabilityService availabilityService,
            IMapper mapper,
            IOptions<BookingOptions> options,
            ILogger<BookingService> logger)
        {
            _bookingRepo = bookingRepo;
            _roomRepo = roomRepo;
            _tripRepo = tripRepo;
            _transportRepo = transportRepo;
            _eventRepo = eventRepo;
            _packageRepo = packageRepo;
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

            if (request.BookingType != BookingType.Hotel)
                return await CreateCatalogBookingAsync(request, userId, userEmail);

            var hotelId = IdEncoder.DecodeId(request.EncodedHotelId!);
            var roomId = IdEncoder.DecodeId(request.EncodedRoomId!);

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
                entity.BookingType = BookingType.Hotel;
                entity.HotelId = room.HotelId;
                entity.RoomId = room.Id;
                entity.Hotel = room.Hotel;
                entity.Room = room;
                entity.Quantity = request.RoomsCount;
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
                    if (booking.BookingType == BookingType.Hotel)
                    {
                        if (booking.Room == null)
                        {
                            _logger.LogCritical(
                                "Booking {BookingId} ({Status}) was paid late (Paymob order {PaymobOrderId}) and its room could not be loaded. Manual refund required.",
                                booking.Id, booking.Status, paymobOrderId);
                            return false;
                        }

                        var available = await _availabilityService.GetAvailableRoomsAsync(
                            booking.Room, booking.CheckIn, booking.CheckOut, excludeBookingId: booking.Id);

                        if (available < Math.Max(1, booking.RoomsBooked))
                        {
                            _logger.LogCritical(
                                "Booking {BookingId} ({Status}) was paid late (Paymob order {PaymobOrderId}) and the rooms are no longer available. Manual refund required.",
                                booking.Id, booking.Status, paymobOrderId);
                            return false;
                        }
                    }
                    else if (!await IsCatalogCapacityAvailableAsync(booking))
                    {
                        _logger.LogCritical(
                            "Booking {BookingId} ({Status}, {BookingType}) was paid late (Paymob order {PaymobOrderId}) and the inventory is no longer available. Manual refund required.",
                            booking.Id, booking.Status, booking.BookingType, paymobOrderId);
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
            BookingStatus? status = null,
            BookingType? bookingType = null)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User id is required.", nameof(userId));

            var bookings = await _bookingRepo.GetAllWithSpecAsync(new BookingFilterSpec(userId, hotelId, from, to, status, bookingType));
            return _mapper.Map<IReadOnlyList<BookingResponseDto>>(bookings);
        }

        public async Task<IReadOnlyList<BookingResponseDto>> GetAdminBookingsAsync(
            int? hotelId = null,
            DateTime? from = null,
            DateTime? to = null,
            BookingStatus? status = null,
            BookingType? bookingType = null)
        {
            var bookings = await _bookingRepo.GetAllWithSpecAsync(new BookingFilterSpec(null, hotelId, from, to, status, bookingType));
            return _mapper.Map<IReadOnlyList<BookingResponseDto>>(bookings);
        }

        private async Task<BookingResponseDto> CreateCatalogBookingAsync(BookingRequestDto request, string userId, string? userEmail)
        {
            var booking = await _bookingRepo.ExecuteInTransactionAsync(async () =>
            {
                var entity = _mapper.Map<Booking>(request);
                entity.BookingType = request.BookingType;
                entity.ExtraBed = false;
                entity.Status = BookingStatus.Pending;
                entity.IsPaid = false;
                entity.MerchantOrderId = $"{MerchantOrderPrefix}-{Guid.NewGuid():N}";
                entity.UserId = userId;
                if (!string.IsNullOrWhiteSpace(userEmail))
                    entity.CustomerEmail = userEmail;

                switch (request.BookingType)
                {
                    case BookingType.Trip:
                        await ApplyTripAsync(entity, request);
                        break;
                    case BookingType.Transport:
                        await ApplyTransportAsync(entity, request);
                        break;
                    case BookingType.Event:
                        await ApplyEventAsync(entity, request);
                        break;
                    case BookingType.Package:
                        await ApplyPackageAsync(entity, request);
                        break;
                    default:
                        throw new ArgumentException("Unsupported booking type.");
                }

                await _bookingRepo.AddAsync(entity);
                await _bookingRepo.SaveChangesAsync();
                return entity;
            }, IsolationLevel.Serializable);

            return _mapper.Map<BookingResponseDto>(booking);
        }

        private async Task ApplyTripAsync(Booking entity, BookingRequestDto request)
        {
            var trip = await _tripRepo.GetByIdAsync(IdEncoder.DecodeId(request.EncodedTripId!))
                ?? throw new NotFoundException("Trip not found");
            if (!trip.IsActive)
                throw new ConflictException("This trip is not available.");

            var serviceDate = request.ServiceDate!.Value.Date;
            EnsureServiceDate(serviceDate, trip.StartDate, trip.EndDate, "trip");

            var guests = request.Adults + request.Children;
            if (guests < 1)
                throw new ArgumentException("At least one guest is required.");

            var booked = await SumTripGuestsAsync(trip.Id, serviceDate, excludeBookingId: null);
            if (booked + guests > trip.MaxCapacity)
            {
                var remaining = Math.Max(0, trip.MaxCapacity - booked);
                throw new ConflictException(remaining == 0
                    ? "This trip is fully booked for the selected date."
                    : $"Only {remaining} place(s) remain on this trip for the selected date.");
            }

            entity.TripId = trip.Id;
            entity.Trip = trip;
            entity.ServiceDate = serviceDate;
            entity.Quantity = guests;
            entity.CheckIn = serviceDate;
            entity.CheckOut = serviceDate;
            entity.RoomsBooked = 0;
            entity.TotalPrice = WithTax((request.Adults * trip.PricePerPerson) + (request.Children * trip.PricePerChild));
        }

        private async Task ApplyTransportAsync(Booking entity, BookingRequestDto request)
        {
            var transport = await _transportRepo.GetByIdAsync(IdEncoder.DecodeId(request.EncodedTransportId!))
                ?? throw new NotFoundException("Transport not found");
            if (!transport.IsActive)
                throw new ConflictException("This transport is not available.");

            var serviceDate = request.ServiceDate!.Value.Date;
            if (serviceDate < DateTime.UtcNow.Date)
                throw new ArgumentException("Service date cannot be in the past.");

            var quantity = request.Quantity;
            var passengers = request.Adults + request.Children;
            var seats = quantity * transport.MaxPassengers;
            if (passengers > seats)
                throw new ArgumentException($"The selected vehicles seat at most {seats} passengers.");

            var booked = await SumTransportVehiclesAsync(transport.Id, serviceDate, excludeBookingId: null);
            if (booked + quantity > transport.AvailableVehicles)
            {
                var remaining = Math.Max(0, transport.AvailableVehicles - booked);
                throw new ConflictException(remaining == 0
                    ? "No vehicles are available for the selected date."
                    : $"Only {remaining} vehicle(s) are available for the selected date.");
            }

            entity.TransportId = transport.Id;
            entity.Transport = transport;
            entity.ServiceDate = serviceDate;
            entity.Quantity = quantity;
            entity.CheckIn = serviceDate;
            entity.CheckOut = serviceDate;
            entity.RoomsBooked = 0;
            entity.TotalPrice = WithTax(quantity * transport.PricePerTrip);
        }

        private async Task ApplyEventAsync(Booking entity, BookingRequestDto request)
        {
            var catalogEvent = await _eventRepo.GetByIdAsync(IdEncoder.DecodeId(request.EncodedEventId!))
                ?? throw new NotFoundException("Event not found");
            if (!catalogEvent.IsActive)
                throw new ConflictException("This event is not available.");

            var start = catalogEvent.EventDate.Date;
            var end = (catalogEvent.EndDate ?? catalogEvent.EventDate).Date;
            if (end < DateTime.UtcNow.Date)
                throw new ConflictException("This event has already ended.");

            if (request.ServiceDate.HasValue)
            {
                var requested = request.ServiceDate.Value.Date;
                if (requested < start || requested > end)
                    throw new ArgumentException("Service date is outside the event dates.");
            }

            var quantity = request.Quantity;
            var booked = await SumEventTicketsAsync(catalogEvent.Id, excludeBookingId: null);
            if (booked + quantity > catalogEvent.TotalTickets)
            {
                var remaining = Math.Max(0, catalogEvent.TotalTickets - booked);
                throw new ConflictException(remaining == 0
                    ? "This event is sold out."
                    : $"Only {remaining} ticket(s) remain for this event.");
            }

            entity.EventId = catalogEvent.Id;
            entity.Event = catalogEvent;
            entity.ServiceDate = catalogEvent.EventDate;
            entity.Quantity = quantity;
            entity.CheckIn = start;
            entity.CheckOut = end;
            entity.RoomsBooked = 0;
            entity.TotalPrice = WithTax(quantity * catalogEvent.TicketPrice);
        }

        private async Task ApplyPackageAsync(Booking entity, BookingRequestDto request)
        {
            var package = await _packageRepo.GetByIdWithSpecAsync(new PackageFilterSpec(IdEncoder.DecodeId(request.EncodedPackageId!)))
                ?? throw new NotFoundException("Package not found");
            if (!package.IsActive)
                throw new ConflictException("This package is not available.");

            var serviceDate = request.ServiceDate?.Date;
            var requiresDate = package.RoomId.HasValue || package.StartDate.HasValue || package.EndDate.HasValue;
            if (requiresDate && !serviceDate.HasValue)
                throw new ArgumentException("Service date is required for this package.");
            if (serviceDate.HasValue)
                EnsureServiceDate(serviceDate.Value, package.StartDate, package.EndDate, "package");

            var quantity = request.Quantity;
            var booked = await SumPackagesAsync(package.Id, excludeBookingId: null);
            if (booked + quantity > package.MaxPackagesAvailable)
            {
                var remaining = Math.Max(0, package.MaxPackagesAvailable - booked);
                throw new ConflictException(remaining == 0
                    ? "This package is sold out."
                    : $"Only {remaining} package(s) remain.");
            }

            Room? room = null;
            DateTime checkIn;
            DateTime checkOut;
            if (package.RoomId is int roomId)
            {
                if (package.NightsCount is not int nights || nights < 1)
                    throw new ArgumentException("This package stay is missing the number of nights.");

                room = await _roomRepo.GetByIdWithSpecAsync(new RoomWithHotelSpec(roomId))
                    ?? throw new NotFoundException("Room not found");
                if (package.HotelId.HasValue && room.HotelId != package.HotelId.Value)
                    throw new ArgumentException("The package room does not belong to the package hotel.");

                checkIn = serviceDate!.Value;
                checkOut = checkIn.AddDays(nights);
                var available = await _availabilityService.GetAvailableRoomsAsync(room, checkIn, checkOut);
                if (available < quantity)
                {
                    throw new ConflictException(available == 0
                        ? "No rooms of this type are available for the package dates."
                        : $"Only {available} room(s) of this type are available for the package dates.");
                }

                entity.RoomsBooked = quantity;
            }
            else
            {
                checkIn = serviceDate ?? DateTime.UtcNow.Date;
                checkOut = checkIn;
                entity.RoomsBooked = 0;
            }

            entity.PackageId = package.Id;
            entity.Package = package;
            entity.HotelId = package.HotelId;
            entity.RoomId = package.RoomId;
            entity.Hotel = room?.Hotel ?? package.Hotel;
            entity.Room = room;
            entity.ServiceDate = serviceDate;
            entity.Quantity = quantity;
            entity.CheckIn = checkIn;
            entity.CheckOut = checkOut;
            entity.TotalPrice = WithTax(quantity * package.PackagePrice);
        }

        private async Task<bool> IsCatalogCapacityAvailableAsync(Booking booking)
        {
            switch (booking.BookingType)
            {
                case BookingType.Trip:
                    if (booking.TripId is not int tripId || booking.ServiceDate is not DateTime tripDate)
                        return false;
                    var trip = booking.Trip ?? await _tripRepo.GetByIdAsync(tripId);
                    if (trip == null)
                        return false;
                    var guests = booking.Adults + booking.Children;
                    var bookedGuests = await SumTripGuestsAsync(trip.Id, tripDate, booking.Id);
                    return bookedGuests + guests <= trip.MaxCapacity;

                case BookingType.Transport:
                    if (booking.TransportId is not int transportId || booking.ServiceDate is not DateTime transportDate)
                        return false;
                    var transport = booking.Transport ?? await _transportRepo.GetByIdAsync(transportId);
                    if (transport == null)
                        return false;
                    var bookedVehicles = await SumTransportVehiclesAsync(transport.Id, transportDate, booking.Id);
                    return bookedVehicles + booking.Quantity <= transport.AvailableVehicles;

                case BookingType.Event:
                    if (booking.EventId is not int eventId)
                        return false;
                    var catalogEvent = booking.Event ?? await _eventRepo.GetByIdAsync(eventId);
                    if (catalogEvent == null)
                        return false;
                    var bookedTickets = await SumEventTicketsAsync(catalogEvent.Id, booking.Id);
                    return bookedTickets + booking.Quantity <= catalogEvent.TotalTickets;

                case BookingType.Package:
                    if (booking.PackageId is not int packageId)
                        return false;
                    var package = booking.Package ?? await _packageRepo.GetByIdAsync(packageId);
                    if (package == null)
                        return false;
                    var bookedPackages = await SumPackagesAsync(package.Id, booking.Id);
                    if (bookedPackages + booking.Quantity > package.MaxPackagesAvailable)
                        return false;
                    if (booking.RoomId is int packageRoomId)
                    {
                        var room = booking.Room ?? await _roomRepo.GetByIdWithSpecAsync(new RoomWithHotelSpec(packageRoomId));
                        if (room == null)
                            return false;
                        var availableRooms = await _availabilityService.GetAvailableRoomsAsync(
                            room, booking.CheckIn, booking.CheckOut, excludeBookingId: booking.Id);
                        if (availableRooms < Math.Max(1, booking.RoomsBooked))
                            return false;
                    }
                    return true;

                default:
                    return false;
            }
        }

        private async Task<int> SumTripGuestsAsync(int tripId, DateTime serviceDate, int? excludeBookingId)
        {
            var bookings = await _bookingRepo.GetAllWithSpecAsync(
                new ActiveTripBookingsSpec(tripId, serviceDate, PendingCutoff, excludeBookingId));
            return bookings.Sum(b => b.Adults + b.Children);
        }

        private async Task<int> SumTransportVehiclesAsync(int transportId, DateTime serviceDate, int? excludeBookingId)
        {
            var bookings = await _bookingRepo.GetAllWithSpecAsync(
                new ActiveTransportBookingsSpec(transportId, serviceDate, PendingCutoff, excludeBookingId));
            return bookings.Sum(b => b.Quantity);
        }

        private async Task<int> SumEventTicketsAsync(int eventId, int? excludeBookingId)
        {
            var bookings = await _bookingRepo.GetAllWithSpecAsync(
                new ActiveEventBookingsSpec(eventId, PendingCutoff, excludeBookingId));
            return bookings.Sum(b => b.Quantity);
        }

        private async Task<int> SumPackagesAsync(int packageId, int? excludeBookingId)
        {
            var bookings = await _bookingRepo.GetAllWithSpecAsync(
                new ActivePackageBookingsSpec(packageId, PendingCutoff, excludeBookingId));
            return bookings.Sum(b => b.Quantity);
        }

        private decimal WithTax(decimal net) =>
            Math.Round(net * (1 + _options.TaxRate), 2, MidpointRounding.AwayFromZero);

        private static void EnsureServiceDate(DateTime serviceDate, DateTime? start, DateTime? end, string itemName)
        {
            if (serviceDate.Date < DateTime.UtcNow.Date)
                throw new ArgumentException("Service date cannot be in the past.");
            if (start.HasValue && serviceDate.Date < start.Value.Date)
                throw new ArgumentException($"This {itemName} is not available on the selected date.");
            if (end.HasValue && serviceDate.Date > end.Value.Date)
                throw new ArgumentException($"This {itemName} is not available on the selected date.");
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
