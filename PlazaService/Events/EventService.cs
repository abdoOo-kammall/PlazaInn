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
        private readonly IGenericRepository<Event> _events;

        public EventService(IGenericRepository<Event> events)
        {
            _events = events;
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

            _events.Delete(entity);
            await CatalogPersistence.SaveOrConflictAsync(
                () => _events.SaveChangesAsync(),
                "This event is included in a package and cannot be deleted.");
        }
    }
}
