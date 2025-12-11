using Shared.DTO.ContactUs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlazaCore.ServiceContract
{
    public interface IContactUsService
    {
        Task<IEnumerable<ContactUsDto>> GetAllContactUs();
        Task<ContactUsDto> GetContactUs(int id);
        Task UpdateContactUs(int id, UpdateContactUsDto contactUsDto);
        Task CreateContactUs(CreateContactUsDto contactUsDto);
        Task DeleteContactUs(int id);





    }
}
