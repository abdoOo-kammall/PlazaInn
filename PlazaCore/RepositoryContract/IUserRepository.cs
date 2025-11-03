using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlazaCore.Entites;

namespace PlazaCore.RepositoryContract
{
    public interface IUserRepository
    {
        Task<IEnumerable<ApplicationUser>> GetAllAsync();
        Task<ApplicationUser?> GetByIdAsync(string id);
        Task AddAsync(ApplicationUser user);
        void Update(ApplicationUser user);
        void Delete(ApplicationUser user);
        Task SaveChangesAsync();
    }

}
