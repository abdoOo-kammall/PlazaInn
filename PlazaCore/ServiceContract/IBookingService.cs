using Shared.DTO.Booking;
using Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlazaCore.ServiceContract
{
    public interface IBookingService
    {
        /// <param name="userId">Authenticated owner of the booking (JWT "Id" claim).</param>
        /// <param name="userEmail">When provided, overrides the customer email sent in the request.</param>
        Task<BookingResponseDto> CreateBookingAsync(BookingRequestDto request, string userId, string? userEmail);

        Task<BookingResponseDto> GetBookingByIdAsync(int bookingId, string userId, bool isAdmin);
        Task<BookingResponseDto> CancelBookingAsync(int bookingId, string userId, bool isAdmin);

        Task<BookingPaymentInfo> PrepareBookingForPaymentAsync(int bookingId, string userId, bool isAdmin);
        Task AttachPaymentReferenceAsync(int bookingId, string paymentReference, string merchantOrderId);

        /// <param name="merchantOrderId">Paymob-verified merchant order id, used when the payment reference was overwritten by a newer attempt.</param>
        Task EnsureCanAccessPaymentAsync(string paymentReference, string? merchantOrderId, string userId, bool isAdmin);

        /// <param name="merchantOrderId">Paymob-verified merchant order id ("PLZ-{bookingId}-{guid}").</param>
        /// <returns>True when the booking is (or already was) confirmed for this Paymob order.</returns>
        Task<bool> ConfirmBookingPaymentAsync(long paymobOrderId, long paidAmountCents, string? merchantOrderId);

        Task<int> CancelExpiredPendingBookingsAsync();

        Task<IReadOnlyList<BookingResponseDto>> GetUserBookingsAsync(
            string userId,
            int? hotelId = null,
            DateTime? from = null,
            DateTime? to = null,
            BookingStatus? status = null);

        Task<IReadOnlyList<BookingResponseDto>> GetAdminBookingsAsync(
            int? hotelId = null,
            DateTime? from = null,
            DateTime? to = null,
            BookingStatus? status = null);

    }

}
