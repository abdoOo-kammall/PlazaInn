using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace PlazaCore.ServiceContract.Account
{
    public interface IEmailService
    {
        public Task SendEmailAsync (string to , string subject, string body, List<IFormFile>? attachments);

    }
}
