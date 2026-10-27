using PlazaCore.Entites;
using Shared.DTO.Package;

namespace PlazaCore.Specification.PackageSpecification
{
    public class PackageFilterSpec : Specification<Package>
    {
        public PackageFilterSpec(PackageFilterDto filter, bool activeOnly)
        {
            var search = string.IsNullOrWhiteSpace(filter.Search) ? null : filter.Search.Trim();
            DateTime? dayStart = filter.Date?.Date;
            DateTime? dayEnd = dayStart?.AddDays(1);
            var minPrice = filter.MinPrice;
            var maxPrice = filter.MaxPrice;
            var duration = filter.DurationDays;
            var hotelOnly = filter.IncludesHotel == true;
            var withoutHotel = filter.IncludesHotel == false;
            var restrictToActive = activeOnly || filter.IsActive == true;
            var restrictToInactive = !activeOnly && filter.IsActive == false;

            Criteria = p =>
                (!restrictToActive || p.IsActive) &&
                (!restrictToInactive || !p.IsActive) &&
                (search == null || p.Name.Contains(search) || p.NameAr.Contains(search)) &&
                (minPrice == null || p.PackagePrice >= minPrice) &&
                (maxPrice == null || p.PackagePrice <= maxPrice) &&
                (duration == null || p.DurationDays == duration) &&
                (!hotelOnly || p.HotelId != null) &&
                (!withoutHotel || p.HotelId == null) &&
                (dayStart == null ||
                    ((p.StartDate == null || p.StartDate < dayEnd) &&
                     (p.EndDate == null || p.EndDate >= dayStart)));

            ApplyOrderBy(p => p.Name);
            AddDetails();
        }

        public PackageFilterSpec(int id)
            : base(p => p.Id == id)
        {
            AddDetails();
        }

        private void AddDetails()
        {
            AddInclude("PackageTrips.Trip");
            AddInclude("PackageTransports.Transport");
            AddInclude("PackageEvents.Event");
            AddInclude(p => p.Hotel!);
            AddInclude(p => p.Room!);
            ApplySplitQuery();
        }
    }
}
