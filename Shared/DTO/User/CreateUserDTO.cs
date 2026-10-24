using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.Enums;

namespace Shared.DTO.User
{
    public class CreateUserDTO
    {
        [Required(ErrorMessage = "Username is required.")]
        [StringLength(50, MinimumLength = 3)]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        public string Email { get; set; }

        [EnumDataType(typeof(Role), ErrorMessage = "Invalid role.")]
        public Role Role { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters long.")]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "Password and confirmation do not match.")]
        public string PasswordConfirmed { get; set; }
    }
}
