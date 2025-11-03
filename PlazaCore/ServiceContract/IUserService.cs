using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using PlazaCore.Entites;
using Shared.DTO.User;
using Shared.Enums;

namespace PlazaCore.ServiceContract
{
    public interface IUserService
    {
        Task<IEnumerable<ApplicationUser>> getAllUsersAsync();
        Task<ApplicationUser> getUserByIdAsync(string id);
        Task<string> getUserRoleAsync(ApplicationUser user);

        Task<IdentityResult> addUserAsync(ApplicationUser user, string password, string role);
        Task<IdentityResult> updateUserAsync(string id, updateUserDTO dto);


        Task<IdentityResult> deleteUserAsync (string id);
    }
}
