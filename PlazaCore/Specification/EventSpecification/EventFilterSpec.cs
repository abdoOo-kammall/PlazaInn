using PlazaCore.Entites;
using Shared.DTO.Event;

namespace PlazaCore.Specification.EventSpecification
{
    public class EventFilterSpec : Specification<Event>
    {
        public EventFilterSpec(EventFilterDto filter, bool activeOnly)
        {
            var search = string.IsNullOrWhiteSpace(filter.Search) ? null : filter.Search.Trim();
            var location = string.IsNullOrWhiteSpace(filter.Location) ? null : filter.Location.Trim();
            DateTime? dayStart = filter.Date?.Date;
            DateTime? dayEnd = dayStart?.AddDays(1);
            var minPrice = filter.MinPrice;
            var maxPrice = filter.MaxPrice;
            var restrictToActive = activeOnly || filter.IsActive == true;
            var restrictToInactive = !activeOnly && filter.IsActive == false;

            Criteria = e =>
                (!restrictToActive || e.IsActive) &&
                (!restrictToInactive || !e.IsActive) &&
                (search == null || e.Name.Contains(search) || e.NameAr.Contains(search)) &&
                (location == null || e.Location.Contains(location) || e.LocationAr.Contains(location)) &&
                (minPrice == null || e.TicketPrice >= minPrice) &&
                (maxPrice == null || e.TicketPrice <= maxPrice) &&
                (dayStart == null || ((e.EndDate ?? e.EventDate) >= dayStart && e.EventDate < dayEnd));

            ApplyOrderBy(e => e.EventDate);
        }
    }

    public class EventsByIdsSpec : Specification<Event>
    {
        public EventsByIdsSpec(IReadOnlyCollection<int> ids)
            : base(e => ids.Contains(e.Id))
        {
        }
    }
}
