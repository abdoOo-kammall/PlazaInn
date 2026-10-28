using PlazaCore.Entites;
using PlazaCore.RepositoryContract;
using PlazaCore.ServiceContract;
using PlazaCore.Specification.TransportSpecification;
using PlazaService.Catalog;
using Shared.DTO.Transport;

namespace PlazaService.Transports
{
    public class TransportService : ITransportService
    {
        private const string DeleteConflictMessage =
            "Cannot delete this transport because it has existing bookings or is included in a package.";

        private readonly IGenericRepository<Transport> _transports;
        private readonly IGenericRepository<Booking> _bookings;
        private readonly IGenericRepository<PackageTransport> _packageTransports;

        public TransportService(
            IGenericRepository<Transport> transports,
            IGenericRepository<Booking> bookings,
            IGenericRepository<PackageTransport> packageTransports)
        {
            _transports = transports;
            _bookings = bookings;
            _packageTransports = packageTransports;
        }

        public async Task<IEnumerable<Transport>> GetTransportsAsync(TransportFilterDto filter, bool activeOnly)
        {
            filter ??= new TransportFilterDto();
            return await _transports.GetAllWithSpecAsync(new TransportFilterSpec(filter, activeOnly));
        }

        public Task<Transport?> GetTransportAsync(int id) => _transports.GetByIdAsync(id);

        public async Task AddTransportAsync(Transport transport)
        {
            await _transports.AddAsync(transport);
            await _transports.SaveChangesAsync();
        }

        public async Task UpdateTransportAsync(Transport transport)
        {
            transport.UpdatedAt = DateTime.UtcNow;
            _transports.Update(transport);
            await _transports.SaveChangesAsync();
        }

        public async Task DeleteTransportAsync(int id)
        {
            var transport = await _transports.GetByIdAsync(id);
            if (transport == null)
                return;

            await CatalogPersistence.EnsureNotReferencedAsync(
                _bookings,
                booking => booking.TransportId == id,
                DeleteConflictMessage);
            await CatalogPersistence.EnsureNotReferencedAsync(
                _packageTransports,
                link => link.TransportId == id,
                DeleteConflictMessage);

            _transports.Delete(transport);
            await CatalogPersistence.SaveOrConflictAsync(
                () => _transports.SaveChangesAsync(),
                DeleteConflictMessage);
        }
    }
}
