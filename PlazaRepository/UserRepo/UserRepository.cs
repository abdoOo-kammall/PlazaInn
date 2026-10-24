using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PlazaCore.Entites;
using PlazaCore.RepositoryContract;

namespace PlazaRepository.UserRepo
{
    public class UserRepository : IUserRepository
    {
        private readonly PlazaDbContext _plazaDbContext;


        public UserRepository(PlazaDbContext plazaDbContext)
        {
            this._plazaDbContext = plazaDbContext;
            
        }
        public async Task AddAsync(ApplicationUser user)
        {
            await _plazaDbContext.Users.AddAsync(user);
        }

        public void Delete(ApplicationUser user)
        {
            _plazaDbContext.Users.Remove(user);
        }

        public async Task<IEnumerable<ApplicationUser>> GetAllAsync()
        {
           return await _plazaDbContext.Users.ToListAsync();
        }

        public async Task<ApplicationUser?> GetByIdAsync(string id)
        {
           return await _plazaDbContext.Users.FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<IReadOnlyDictionary<string, string>> GetUserRoleNamesAsync()
        {
            var pairs = await (from userRole in _plazaDbContext.UserRoles
                               join role in _plazaDbContext.Roles on userRole.RoleId equals role.Id
                               select new { userRole.UserId, role.Name })
                              .ToListAsync();

            return pairs
                .Where(p => p.Name != null)
                .GroupBy(p => p.UserId)
                .ToDictionary(g => g.Key, g => g.First().Name!);
        }

        public async Task SaveChangesAsync()
        {
            await _plazaDbContext.SaveChangesAsync();
        }


        public void Update(ApplicationUser user)
        {
            _plazaDbContext.Users.Update(user);
        }

    }
}
