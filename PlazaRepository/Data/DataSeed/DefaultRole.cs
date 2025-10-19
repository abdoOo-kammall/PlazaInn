using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace PlazaRepository.Data.DataSeed
{
    public static class DefaultRole
    {
        public static async Task SeedAsync(RoleManager<IdentityRole> roleManager)
        {
            if (!await roleManager.RoleExistsAsync("Admin"))
                await roleManager.CreateAsync(new IdentityRole("Admin"));

            if (!await roleManager.RoleExistsAsync("Client"))
                await roleManager.CreateAsync(new IdentityRole("Client"));
        }
    }
}
