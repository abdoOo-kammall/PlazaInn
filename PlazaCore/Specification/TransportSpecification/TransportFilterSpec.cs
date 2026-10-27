using PlazaCore.Entites;
using Shared.DTO.Transport;

namespace PlazaCore.Specification.TransportSpecification
{
    public class TransportFilterSpec : Specification<Transport>
    {
        public TransportFilterSpec(TransportFilterDto filter, bool activeOnly)
        {
            var search = string.IsNullOrWhiteSpace(filter.Search) ? null : filter.Search.Trim();
            var vehicleType = string.IsNullOrWhiteSpace(filter.VehicleType) ? null : filter.VehicleType.Trim();
            var pickup = string.IsNullOrWhiteSpace(filter.PickupLocation) ? null : filter.PickupLocation.Trim();
            var dropoff = string.IsNullOrWhiteSpace(filter.DropoffLocation) ? null : filter.DropoffLocation.Trim();
            var passengers = filter.Passengers;
            var minPrice = filter.MinPrice;
            var maxPrice = filter.MaxPrice;
            var restrictToActive = activeOnly || filter.IsActive == true;
            var restrictToInactive = !activeOnly && filter.IsActive == false;

            Criteria = t =>
                (!restrictToActive || t.IsActive) &&
                (!restrictToInactive || !t.IsActive) &&
                (search == null || t.Name.Contains(search) || t.NameAr.Contains(search)) &&
                (vehicleType == null || t.VehicleType.Contains(vehicleType) || t.VehicleTypeAr.Contains(vehicleType)) &&
                (pickup == null || t.PickupLocation.Contains(pickup) || t.PickupLocationAr.Contains(pickup)) &&
                (dropoff == null || t.DropoffLocation.Contains(dropoff) || t.DropoffLocationAr.Contains(dropoff)) &&
                (passengers == null || t.MaxPassengers >= passengers) &&
                (minPrice == null || t.PricePerTrip >= minPrice) &&
                (maxPrice == null || t.PricePerTrip <= maxPrice);

            ApplyOrderBy(t => t.Name);
        }
    }

    public class TransportsByIdsSpec : Specification<Transport>
    {
        public TransportsByIdsSpec(IReadOnlyCollection<int> ids)
            : base(t => ids.Contains(t.Id))
        {
        }
    }
}
