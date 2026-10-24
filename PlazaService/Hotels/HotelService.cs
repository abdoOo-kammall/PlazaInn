    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using PlazaCore.Entites;
    using PlazaCore.RepositoryContract;
    using PlazaCore.ServiceContract;
using PlazaCore.Specification.HotelSpecification;

namespace PlazaService.Hotels
    {
        public class HotelService : IHotelService
        {
            private readonly IGenericRepository<Hotel> _HotelRepo;

            public HotelService( IGenericRepository<Hotel> genericRepository)
            {
                this._HotelRepo = genericRepository;
            }
            public async Task  AddHotelAsync(Hotel hotel)
            {
                await _HotelRepo.AddAsync(hotel);
                await _HotelRepo.SaveChangesAsync();
            }

            public async Task DeleteHotelAsync(int id)
            {
                var hotel = await _HotelRepo.GetByIdAsync(id);  
                if (hotel != null) { 
                     _HotelRepo.Delete(hotel);
                     await _HotelRepo.SaveChangesAsync();
                }
            }

            public async Task<IEnumerable<Hotel>> GetAllHotelsAsync()
            {
            var spec = new HotelWithImagesAndRoomsSpec();
            var hotels =  await _HotelRepo.GetAllWithSpecAsync(spec);
            return hotels;
            }

            public async Task<Hotel?> GetHotelAsync(int id)
            {
            var spec = new HotelWithImagesAndRoomsSpec(id);

            return await _HotelRepo.GetByIdWithSpecAsync(spec);

            }

            public async Task<IReadOnlyList<Hotel>> GetHotelsByIdsAsync(IReadOnlyCollection<int> ids)
            {
                if (ids.Count == 0)
                    return Array.Empty<Hotel>();

                var hotels = await _HotelRepo.GetAllWithSpecAsync(new HotelsByIdsSpec(ids));
                return hotels.ToList();
            }

            public async Task UpdateHotelAsync(Hotel hotel)
            {
                 _HotelRepo.Update(hotel);
                await _HotelRepo.SaveChangesAsync();
            }
        }
    }
