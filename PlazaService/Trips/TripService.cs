using PlazaCore.Entites;
using PlazaCore.RepositoryContract;
using PlazaCore.ServiceContract;
using PlazaCore.Specification.TripSpecification;
using PlazaService.Catalog;
using Shared.DTO.Trip;

namespace PlazaService.Trips
{
    public class TripService : ITripService
    {
        private readonly IGenericRepository<Trip> _trips;

        public TripService(IGenericRepository<Trip> trips)
        {
            _trips = trips;
        }

        public async Task<IEnumerable<Trip>> GetTripsAsync(TripFilterDto filter, bool activeOnly)
        {
            filter ??= new TripFilterDto();
            return await _trips.GetAllWithSpecAsync(new TripFilterSpec(filter, activeOnly));
        }

        public Task<Trip?> GetTripAsync(int id) => _trips.GetByIdAsync(id);

        public async Task AddTripAsync(Trip trip)
        {
            await _trips.AddAsync(trip);
            await _trips.SaveChangesAsync();
        }

        public async Task UpdateTripAsync(Trip trip)
        {
            trip.UpdatedAt = DateTime.UtcNow;
            _trips.Update(trip);
            await _trips.SaveChangesAsync();
        }

        public async Task DeleteTripAsync(int id)
        {
            var trip = await _trips.GetByIdAsync(id);
            if (trip == null)
                return;

            _trips.Delete(trip);
            await CatalogPersistence.SaveOrConflictAsync(
                () => _trips.SaveChangesAsync(),
                "This trip is included in a package and cannot be deleted.");
        }
    }
}
