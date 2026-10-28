using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using PlazaCore.Entites;
using PlazaCore.RepositoryContract;
using PlazaCore.ServiceContract;
using PlazaCore.Specification.RoomSpecification;
using PlazaService.Catalog;

namespace PlazaService.Hotels
{
    public class RoomService : IRoomService
    {
        private const string DeleteConflictMessage =
            "Cannot delete this room because it has existing bookings or packages.";

        private readonly IGenericRepository<Room> _roomRepo;
        private readonly IGenericRepository<Booking> _bookings;
        private readonly IGenericRepository<Package> _packages;

        public RoomService(
            IGenericRepository<Room> genericRepository,
            IGenericRepository<Booking> bookings,
            IGenericRepository<Package> packages)
        {
            _roomRepo = genericRepository;
            _bookings = bookings;
            _packages = packages;
        }
        public async Task AddRoomAsync(Room room)
        {
            await _roomRepo.AddAsync(room);
            await _roomRepo.SaveChangesAsync();
        }
        public async Task<IEnumerable<Room>> GetRoomByHotelIdAsync(int id) {
            var spec =  new RoomByHotelIdSpec(id);
            return await _roomRepo.GetAllWithSpecAsync(spec);
        
        }
        public async Task DeleteRoomAsync(int id)
        {
            var room = await _roomRepo.GetByIdAsync(id);
            if (room == null)
                return;

            await CatalogPersistence.EnsureNotReferencedAsync(
                _bookings,
                booking => booking.RoomId == id,
                DeleteConflictMessage);
            await CatalogPersistence.EnsureNotReferencedAsync(
                _packages,
                package => package.RoomId == id,
                DeleteConflictMessage);

            _roomRepo.Delete(room);
            await CatalogPersistence.SaveOrConflictAsync(
                () => _roomRepo.SaveChangesAsync(),
                DeleteConflictMessage);
        }

        public async Task<IEnumerable<Room>> GetAllRoomsAsync()
        {
            var spec = new RoomWithImagesSpec();
           return await _roomRepo.GetAllWithSpecAsync(spec);
        }

        public async Task<Room?> GetRoomAsync(int id)
        {
            var spec = new RoomWithImagesSpec(id);
            return await _roomRepo.GetByIdWithSpecAsync(spec);
        }

        public async Task UpdateRoomAsync(Room room)
        {
            _roomRepo.Update(room);
            await _roomRepo.SaveChangesAsync();
        }

        
    }
}
