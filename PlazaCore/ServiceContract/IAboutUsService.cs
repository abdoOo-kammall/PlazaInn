using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.DTO.AboutUs;

namespace PlazaCore.ServiceContract
{
    public interface IAboutUsService
    {
        Task<IEnumerable<AboutUsDto>> getAllAboutUsImages();
       
        Task updateAboutUsImages(UpdateAboutUsDto aboutUs);
      




    }
}
