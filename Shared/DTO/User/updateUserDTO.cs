using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

        [EnumDataType(typeof(Role), ErrorMessage = "Invalid role.")]
        public Role Role { get; set; }

        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters long.")]
        public string? Password { get; set; }
        public string? PasswordConfirmed { get; set; }
    }
}
