using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PlazaCore.Entites;
using PlazaCore.RepositoryContract;
using Shared.Helper;

namespace PlazaService.Hotels
{
    /// <summary>
    /// Loads each Image.Url into the lookup used by file mapping so Cloudinary URLs survive an app restart.
    /// </summary>
    public class StoredImageLocationWarmup : IHostedService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<StoredImageLocationWarmup> _logger;

        public StoredImageLocationWarmup(IServiceScopeFactory scopeFactory, ILogger<StoredImageLocationWarmup> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var images = await scope.ServiceProvider.GetRequiredService<IGenericRepository<Image>>().GetAllAsync();
                foreach (var image in images)
                    Methods.RememberStoredLocation(image.Id, image.Url);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Stored image locations could not be cached. Existing wwwroot files still resolve from disk.");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
