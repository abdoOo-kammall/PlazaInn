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
        private const string DeleteConflictMessage =
            "Cannot delete this trip because it has existing bookings or is included in a package.";

        private readonly IGenericRepository<Trip> _trips;
        private readonly IGenericRepository<Booking> _bookings;
        private readonly IGenericRepository<PackageTrip> _packageTrips;

        public TripService(
            IGenericRepository<Trip> trips,
            IGenericRepository<Booking> bookings,
            IGenericRepository<PackageTrip> packageTrips)
        {
            _trips = trips;
            _bookings = bookings;
            _packageTrips = packageTrips;
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

            await CatalogPersistence.EnsureNotReferencedAsync(
                _bookings,
                booking => booking.TripId == id,
                DeleteConflictMessage);
            await CatalogPersistence.EnsureNotReferencedAsync(
                _packageTrips,
                link => link.TripId == id,
                DeleteConflictMessage);

            _trips.Delete(trip);
            await CatalogPersistence.SaveOrConflictAsync(
                () => _trips.SaveChangesAsync(),
                DeleteConflictMessage);
        }
    }
}
