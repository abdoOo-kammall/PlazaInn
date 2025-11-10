using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plaza.DTO.Account;
using PlazaCore.Entites;
using Shared.DTO.Account;

namespace PlazaCore.ServiceContract.Account
{
    public interface IAuthService
    {
        public Task RegisterAsync(RegisterDTO registerDTO);
        public Task<LoginResultDTO> LoginAsync(LoginDTO loginDTO);
        public Task ForgotPasswordAsync(string email);
        public Task ResetPasswordAsync(string email, string token, string newPassword);
    }
}
