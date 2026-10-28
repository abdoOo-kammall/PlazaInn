using System.Text.Json;
using PlazaCore.Entites;
using PlazaCore.RepositoryContract;
using PlazaCore.ServiceContract;
using PlazaCore.Specification.HotelSpecification;
using PlazaService.Catalog;

namespace PlazaService.Hotels
{
    public class HotelService : IHotelService
    {
        private const string DeleteConflictMessage =
            "Cannot delete this hotel because it has existing bookings or packages.";

        private readonly IGenericRepository<Hotel> _hotelRepo;
        private readonly IGenericRepository<Room> _rooms;
        private readonly IGenericRepository<InsightHotel> _insights;
        private readonly IGenericRepository<Booking> _bookings;
        private readonly IGenericRepository<Package> _packages;
        private readonly IFileService _files;

        public HotelService(
            IGenericRepository<Hotel> genericRepository,
            IGenericRepository<Room> rooms,
            IGenericRepository<InsightHotel> insights,
            IGenericRepository<Booking> bookings,
            IGenericRepository<Package> packages,
            IFileService files)
        {
            _hotelRepo = genericRepository;
            _rooms = rooms;
            _insights = insights;
            _bookings = bookings;
            _packages = packages;
            _files = files;
        }

        public async Task AddHotelAsync(Hotel hotel)
        {
            await _hotelRepo.AddAsync(hotel);
            await _hotelRepo.SaveChangesAsync();
        }

        public async Task DeleteHotelAsync(int id)
        {
            await _hotelRepo.ExecuteInTransactionAsync(async () =>
            {
                var hotel = await _hotelRepo.GetByIdWithSpecAsync(new HotelWithImagesAndRoomsSpec(id));
                if (hotel == null)
                    return false;

                var hotelId = hotel.Id;
                var roomIds = hotel.Rooms.Select(room => room.Id).ToList();

                await CatalogPersistence.EnsureNotReferencedAsync(
                    _bookings,
                    booking => booking.HotelId == hotelId || roomIds.Contains(booking.RoomId ?? 0),
                    DeleteConflictMessage);
                await CatalogPersistence.EnsureNotReferencedAsync(
                    _packages,
                    package => package.HotelId == hotelId || roomIds.Contains(package.RoomId ?? 0),
                    DeleteConflictMessage);

                var imageIds = CollectImageIds(hotel);

                foreach (var room in hotel.Rooms.ToList())
                    _rooms.Delete(room);

                if (hotel.InsightHotel != null)
                    _insights.Delete(hotel.InsightHotel);

                _hotelRepo.Delete(hotel);
                await CatalogPersistence.SaveOrConflictAsync(
                    () => _hotelRepo.SaveChangesAsync(),
                    DeleteConflictMessage);

                foreach (var imageId in imageIds)
                    await _files.DeleteFileAsync(imageId);

                return true;
            });
        }

        public async Task<IEnumerable<Hotel>> GetAllHotelsAsync(decimal? minPrice = null, decimal? maxPrice = null)
        {
            var spec = minPrice == null && maxPrice == null
                ? new HotelWithImagesAndRoomsSpec()
                : new HotelWithImagesAndRoomsSpec(minPrice, maxPrice);
            return await _hotelRepo.GetAllWithSpecAsync(spec);
        }

        public async Task<Hotel?> GetHotelAsync(int id)
        {
            var spec = new HotelWithImagesAndRoomsSpec(id);
            return await _hotelRepo.GetByIdWithSpecAsync(spec);
        }

        public async Task<IReadOnlyList<Hotel>> GetHotelsByIdsAsync(IReadOnlyCollection<int> ids)
        {
            if (ids.Count == 0)
                return Array.Empty<Hotel>();

            var hotels = await _hotelRepo.GetAllWithSpecAsync(new HotelsByIdsSpec(ids));
            return hotels.ToList();
        }

        public async Task UpdateHotelAsync(Hotel hotel)
        {
            _hotelRepo.Update(hotel);
            await _hotelRepo.SaveChangesAsync();
        }

        private static List<int> CollectImageIds(Hotel hotel)
        {
            var ids = new List<int>();
            ids.AddRange(ReadStoredIds(hotel.ImageIds));
            ids.AddRange(ReadStoredIds(hotel.LogoId));

            foreach (var room in hotel.Rooms)
                ids.AddRange(ReadStoredIds(room.ImageIds));

            var insight = hotel.InsightHotel;
            if (insight != null)
            {
                ids.AddRange(ReadStoredIds(insight.RestaurantImageIds));
                ids.AddRange(ReadStoredIds(insight.CafeImageIds));
                ids.AddRange(ReadStoredIds(insight.RestaurantMenuPdfIds));
                ids.AddRange(ReadStoredIds(insight.CafeMenuPdfIds));
            }

            return ids.Where(id => id > 0).Distinct().ToList();
        }

        private static IEnumerable<int> ReadStoredIds(string? stored)
        {
            if (string.IsNullOrWhiteSpace(stored))
                return Array.Empty<int>();

            var trimmed = stored.Trim();
            if (int.TryParse(trimmed, out var singleId))
                return singleId > 0 ? new[] { singleId } : Array.Empty<int>();

            try
            {
                return JsonSerializer.Deserialize<List<int>>(trimmed) ?? new List<int>();
            }
            catch (JsonException)
            {
                return Array.Empty<int>();
            }
        }
    }
}
