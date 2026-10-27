using PlazaCore.Entites;
using Shared.DTO.Package;

namespace PlazaCore.ServiceContract
{
    public class PackageComposition
    {
        public int? HotelId { get; init; }
        public int? RoomId { get; init; }
        public int? NightsCount { get; init; }
        public IReadOnlyCollection<int> TripIds { get; init; } = Array.Empty<int>();
        public IReadOnlyCollection<int> TransportIds { get; init; } = Array.Empty<int>();
        public IReadOnlyCollection<int> EventIds { get; init; } = Array.Empty<int>();
    }

    public interface IPackageService
    {
        Task<IEnumerable<Package>> GetPackagesAsync(PackageFilterDto filter, bool activeOnly);
        Task<Package?> GetPackageAsync(int id);
        Task AddPackageAsync(Package package, PackageComposition composition);
        Task UpdatePackageAsync(Package package, PackageComposition composition);
        Task DeletePackageAsync(int id);
    }
}
