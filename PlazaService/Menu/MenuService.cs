using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using PlazaCore.ServiceContract;
using Shared.Enums;

namespace PlazaService.Menu
{
    public class MenuService : IMenuService
    {
        private readonly IWebHostEnvironment _env;

        public MenuService(IWebHostEnvironment webHost)
        {
            this._env = webHost;
        }
        public async Task<string> SaveMenuPdfAsync(IFormFile file, MenuType entityType)
        {
            if (Path.GetExtension(file.FileName).ToLower() != ".pdf")
                throw new Exception("Only PDF allowed");

            var root = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
            var folder = Path.Combine(root, "menus", entityType.ToString());
            Directory.CreateDirectory(folder);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var path = Path.Combine(folder, fileName);

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return Path.Combine("menus", entityType.ToString(), fileName).Replace("\\", "/");
        }

    }
}
