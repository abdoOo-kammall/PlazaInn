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

namespace PlazaService.Hotels
{
    public class RoomService : IRoomService
    {
        private readonly IGenericRepository<Room> _roomRepo;

        public RoomService(IGenericRepository<Room> genericRepository)
        {
            this._roomRepo = genericRepository;
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
            if (room != null) {
                 _roomRepo.Delete(room);
                await _roomRepo.SaveChangesAsync();
            }
      
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
