using PlazaCore.Entites;
using PlazaCore.RepositoryContract;
using PlazaCore.ServiceContract;
using PlazaCore.Specification.EventSpecification;
using PlazaService.Catalog;
using Shared.DTO.Event;

namespace PlazaService.Events
{
    public class EventService : IEventService
    {
        private const string DeleteConflictMessage =
            "Cannot delete this event because it has existing bookings or is included in a package.";

        private readonly IGenericRepository<Event> _events;
        private readonly IGenericRepository<Booking> _bookings;
        private readonly IGenericRepository<PackageEvent> _packageEvents;

        public EventService(
            IGenericRepository<Event> events,
            IGenericRepository<Booking> bookings,
            IGenericRepository<PackageEvent> packageEvents)
        {
            _events = events;
            _bookings = bookings;
            _packageEvents = packageEvents;
        }

        public async Task<IEnumerable<Event>> GetEventsAsync(EventFilterDto filter, bool activeOnly)
        {
            filter ??= new EventFilterDto();
            return await _events.GetAllWithSpecAsync(new EventFilterSpec(filter, activeOnly));
        }

        public Task<Event?> GetEventAsync(int id) => _events.GetByIdAsync(id);

        public async Task AddEventAsync(Event entity)
        {
            await _events.AddAsync(entity);
            await _events.SaveChangesAsync();
        }

        public async Task UpdateEventAsync(Event entity)
        {
            entity.UpdatedAt = DateTime.UtcNow;
            _events.Update(entity);
            await _events.SaveChangesAsync();
        }

        public async Task DeleteEventAsync(int id)
        {
            var entity = await _events.GetByIdAsync(id);
            if (entity == null)
                return;

            await CatalogPersistence.EnsureNotReferencedAsync(
                _bookings,
                booking => booking.EventId == id,
                DeleteConflictMessage);
            await CatalogPersistence.EnsureNotReferencedAsync(
                _packageEvents,
                link => link.EventId == id,
                DeleteConflictMessage);

            _events.Delete(entity);
            await CatalogPersistence.SaveOrConflictAsync(
                () => _events.SaveChangesAsync(),
                DeleteConflictMessage);
        }
    }
}
