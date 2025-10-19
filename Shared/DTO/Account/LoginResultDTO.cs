using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DTO.Account
{
    public class LoginResultDTO
    {
        public bool Success { get; set; }
        public string Token { get; set; }
        public string Message { get; set; }
    }
}
