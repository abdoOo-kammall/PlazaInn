using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using PlazaCore.Entites;

namespace PlazaRepository.Data.DataSeed
{
    public static class DefaultUser
    {
        public const string DefaultAdminEmail = "admin@plaza.com";
        public const string DefaultAdminPassword = "Admin@123";
        private const string AdminRole = "Admin";

        public static async Task SeedAsync(UserManager<ApplicationUser> userManager, string email, string password)
        {
            // Older databases stored the admin with UserName = email but a blank Email, so also look it up by name.
            var user = await userManager.FindByEmailAsync(email)
                       ?? await userManager.FindByNameAsync(email);

            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, password);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Failed to seed default admin '{email}': {errors}");
                }
            }
            else if (!string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase))
            {
                user.Email = email;
                user.EmailConfirmed = true;
                var updateResult = await userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Failed to fix default admin email '{email}': {errors}");
                }
            }

            if (!await userManager.IsInRoleAsync(user, AdminRole))
            {
                await userManager.AddToRoleAsync(user, AdminRole);
            }
        }
    }
}
