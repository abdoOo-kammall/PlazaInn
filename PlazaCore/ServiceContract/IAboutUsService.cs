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
        //Task<AboutUsDto> getAboutUsImagesById(int id);
        Task addAboutUsImages(CreateAboutUsDto aboutUs);
        Task updateAboutUsImages(int id , UpdateAboutUsDto aboutUs);
        //Task deleteAboutUsImages(int id);




    }
}
