using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PlazaCore.ServiceContract;
using Shared.Options;

namespace PlazaService.BookingService
{
    /// <summary>
    /// Periodically cancels pending bookings whose payment hold window has elapsed so they stop blocking inventory.
    /// </summary>
    public class ExpiredBookingsCleanupService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly BookingOptions _options;
        private readonly ILogger<ExpiredBookingsCleanupService> _logger;

        public ExpiredBookingsCleanupService(
            IServiceScopeFactory scopeFactory,
            IOptions<BookingOptions> options,
            ILogger<ExpiredBookingsCleanupService> logger)
        {
            _scopeFactory = scopeFactory;
            _options = options.Value;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var interval = TimeSpan.FromMinutes(Math.Max(1, _options.CleanupIntervalMinutes));
            using var timer = new PeriodicTimer(interval);

            try
            {
                do
                {
                    await CancelExpiredBookingsAsync();
                }
                while (await timer.WaitForNextTickAsync(stoppingToken));
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
            }
        }

        private async Task CancelExpiredBookingsAsync()
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();

                var cancelled = await bookingService.CancelExpiredPendingBookingsAsync();
                if (cancelled > 0)
                    _logger.LogInformation("Cancelled {Count} expired pending booking(s).", cancelled);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to cancel expired pending bookings.");
            }
        }
    }
}
