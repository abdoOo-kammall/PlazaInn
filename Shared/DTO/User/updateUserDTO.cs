using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.Enums;

namespace Shared.DTO.User
{
    public class updateUserDTO 
    {
        public string UserName { get; set; }
        //public string Email { get; set; }
        public string Role { get; set; }

        public string? Password { get; set; }
        public string? PasswordConfirmed { get; set; }
    }
}
