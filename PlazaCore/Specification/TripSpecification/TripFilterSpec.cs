using PlazaCore.Entites;
using Shared.DTO.Trip;

namespace PlazaCore.Specification.TripSpecification
{
    public class TripFilterSpec : Specification<Trip>
    {
        public TripFilterSpec(TripFilterDto filter, bool activeOnly)
        {
            var city = string.IsNullOrWhiteSpace(filter.City) ? null : filter.City.Trim();
            var search = string.IsNullOrWhiteSpace(filter.Search) ? null : filter.Search.Trim();
            DateTime? dayStart = filter.Date?.Date;
            DateTime? dayEnd = dayStart?.AddDays(1);
            var minPrice = filter.MinPrice;
            var maxPrice = filter.MaxPrice;
            var restrictToActive = activeOnly || filter.IsActive == true;
            var restrictToInactive = !activeOnly && filter.IsActive == false;

            Criteria = t =>
                (!restrictToActive || t.IsActive) &&
                (!restrictToInactive || !t.IsActive) &&
                (city == null || t.City.Contains(city) || t.CityAr.Contains(city)) &&
                (search == null || t.Name.Contains(search) || t.NameAr.Contains(search)) &&
                (minPrice == null || t.PricePerPerson >= minPrice) &&
                (maxPrice == null || t.PricePerPerson <= maxPrice) &&
                (dayStart == null ||
                    ((t.StartDate == null || t.StartDate < dayEnd) &&
                     (t.EndDate == null || t.EndDate >= dayStart)));

            ApplyOrderBy(t => t.Name);
        }
    }

    public class TripsByIdsSpec : Specification<Trip>
    {
        public TripsByIdsSpec(IReadOnlyCollection<int> ids)
            : base(t => ids.Contains(t.Id))
        {
        }
    }
}
