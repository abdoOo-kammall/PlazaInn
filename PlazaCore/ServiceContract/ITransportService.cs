using PlazaCore.Entites;
using Shared.DTO.Transport;

namespace PlazaCore.ServiceContract
{
    public interface ITransportService
    {
        Task<IEnumerable<Transport>> GetTransportsAsync(TransportFilterDto filter, bool activeOnly);
        Task<Transport?> GetTransportAsync(int id);
        Task AddTransportAsync(Transport transport);
        Task UpdateTransportAsync(Transport transport);
        Task DeleteTransportAsync(int id);
    }
}
