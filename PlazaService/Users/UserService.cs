using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using PlazaCore.Entites;
using PlazaCore.RepositoryContract;
using PlazaCore.ServiceContract;
using Shared.DTO.User;
using Shared.Enums;

namespace PlazaService.Users
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepo;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserService(IUserRepository userRepository , UserManager<ApplicationUser> userManager ,RoleManager<IdentityRole> roleManager)
        {
            this._userRepo = userRepository;
            this._userManager = userManager;
            this._roleManager = roleManager;
        }

        public async Task<IdentityResult> addUserAsync(ApplicationUser user, string password, string role)
        {
            //if (!await _roleManager.RoleExistsAsync(role)) ;
            //await _roleManager.CreateAsync(new IdentityRole(role));
            if (!Enum.IsDefined(typeof(Role), role))
                throw new ArgumentException($"Invalid role: {role}");
            var result = await _userManager.CreateAsync(user , password);
            if (!result.Succeeded)
                return result;

            await _userManager.AddToRoleAsync(user, role.ToString());
            return result;
        }

        public async Task<IdentityResult> deleteUserAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) throw new Exception("User not found.");
           var result = await _userManager.DeleteAsync(user);
            return result;

        }

        public async Task<IEnumerable<ApplicationUser>> getAllUsersAsync()
        {
            return await _userRepo.GetAllAsync();
        }

        public async Task<ApplicationUser> getUserByIdAsync(string id)
        {
            return await _userRepo.GetByIdAsync(id);
        }

        public async Task<string> getUserRoleAsync(ApplicationUser user)
        {
            var role = await _userManager.GetRolesAsync(user);
            return role.FirstOrDefault();
        }

        public async Task<IdentityResult> updateUserAsync(string id, updateUserDTO dto)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                throw new Exception("User not found.");

            if (!string.IsNullOrWhiteSpace(dto.UserName))
                user.UserName = dto.UserName;

            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                if (dto.Password != dto.PasswordConfirmed)
                    throw new Exception("Password and confirmation do not match.");

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var passwordResult = await _userManager.ResetPasswordAsync(user, token, dto.Password);
                if (!passwordResult.Succeeded)
                    return passwordResult;
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            var newRole = dto.Role;

            if (!currentRoles.Contains(newRole))
            {
                if (currentRoles.Any())
                    await _userManager.RemoveFromRolesAsync(user, currentRoles);

                if (!await _roleManager.RoleExistsAsync(newRole))
                    await _roleManager.CreateAsync(new IdentityRole(newRole));

                await _userManager.AddToRoleAsync(user, newRole);
            }

            var result = await _userManager.UpdateAsync(user);
            return result;
        }

    }
}
