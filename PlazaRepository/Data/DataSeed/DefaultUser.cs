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
        public static async Task SeedAsync(UserManager<ApplicationUser> userManager) {

            var defaultUser = new ApplicationUser() {
                UserName = "admin@plaza.com",
                Email = "admin@plaza.com",
                EmailConfirmed = true
            };
        
            var user = await userManager.FindByEmailAsync(defaultUser.Email);
            if ( user == null)
            {
                var result = await userManager.CreateAsync(defaultUser , "Admin@123");
                if (result.Succeeded) {
                    await userManager.AddToRoleAsync(defaultUser, "Admin");
                }
            }

        }
    }
}
