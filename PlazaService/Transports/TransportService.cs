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
        private readonly IGenericRepository<Transport> _transports;

        public TransportService(IGenericRepository<Transport> transports)
        {
            _transports = transports;
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

            _transports.Delete(transport);
            await CatalogPersistence.SaveOrConflictAsync(
                () => _transports.SaveChangesAsync(),
                "This transport is included in a package and cannot be deleted.");
        }
    }
}
