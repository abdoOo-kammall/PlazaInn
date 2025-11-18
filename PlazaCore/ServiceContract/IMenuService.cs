using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Shared.Enums;

namespace PlazaCore.ServiceContract
{
    public interface IMenuService
    {
         Task<string> SaveMenuPdfAsync(IFormFile file, MenuType entityType);
    }
}
