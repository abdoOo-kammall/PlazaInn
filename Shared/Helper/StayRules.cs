using System;

namespace Shared.Helper
{
    public static class StayRules
    {
        public const int MaxStayNights = 30;

        public static int GetNights(DateTime checkIn, DateTime checkOut) => (checkOut.Date - checkIn.Date).Days;

        /// <returns>A user-facing error message, or null when the stay length is valid.</returns>
        public static string? GetStayLengthError(DateTime checkIn, DateTime checkOut)
        {
            var nights = GetNights(checkIn, checkOut);
            if (nights <= 0)
                return "Check-out must be at least one night after check-in.";
            if (nights > MaxStayNights)
                return $"A stay cannot exceed {MaxStayNights} nights.";
            return null;
        }
    }
}
