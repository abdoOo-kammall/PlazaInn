using PlazaCore.Entites;
using Shared.DTO.Event;

namespace PlazaCore.ServiceContract
{
    public interface IEventService
    {
        Task<IEnumerable<Event>> GetEventsAsync(EventFilterDto filter, bool activeOnly);
        Task<Event?> GetEventAsync(int id);
        Task AddEventAsync(Event entity);
        Task UpdateEventAsync(Event entity);
        Task DeleteEventAsync(int id);
    }
}
