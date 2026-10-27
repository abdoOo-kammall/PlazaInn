using PlazaCore.Entites;
using Shared.DTO.Trip;

namespace PlazaCore.ServiceContract
{
    public interface ITripService
    {
        Task<IEnumerable<Trip>> GetTripsAsync(TripFilterDto filter, bool activeOnly);
        Task<Trip?> GetTripAsync(int id);
        Task AddTripAsync(Trip trip);
        Task UpdateTripAsync(Trip trip);
        Task DeleteTripAsync(int id);
    }
}
