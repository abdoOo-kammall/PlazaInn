using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Plaza.DTO.Account;
using PlazaCore.Entites;
using Microsoft.AspNetCore.Identity;

using PlazaCore.ServiceContract.Account;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using AutoMapper;
using Shared.DTO.Account;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PlazaService.Account
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        public AuthService(UserManager<ApplicationUser> userManager  ,IJwtTokenGenerator jwtTokenGenerator ,IMapper mapper , IEmailService emailService )
        {
            this._userManager = userManager;
            this._jwtTokenGenerator = jwtTokenGenerator;
            this._mapper = mapper;
            this._emailService = emailService;
        }

        public async Task ForgotPasswordAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) { 
               throw new ArgumentNullException("email not found");
            }
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetLink = $"http://localhost:4200/reset-password?email={email}&token={Uri.EscapeDataString(token)}";
            await _emailService.SendEmailAsync(email, "Reset Password",
                $"Click <a href='{resetLink}'>here</a> to reset your password.");


        }

        public async Task<LoginResultDTO> LoginAsync(LoginDTO loginDTO)
        {
            var user = await _userManager.FindByEmailAsync(loginDTO.Email);
            if (user == null ||!await _userManager.CheckPasswordAsync(user, loginDTO.Password)) {
                return new LoginResultDTO { Success = false, Message = "Invalid user or password" };
            }
            var token = await _jwtTokenGenerator.GenerateToken(user);
            return new LoginResultDTO
            {
                Success = true,
                Token = token,
                Message = "Login successful."
            };
        }

        public async Task RegisterAsync(RegisterDTO registerDTO)
        {
                var emailExist = await _userManager.FindByEmailAsync(registerDTO.Email);
            if (emailExist != null) {
                throw new ArgumentException("Email is already registered.");
            }
            var user = _mapper.Map<ApplicationUser>(registerDTO);

            var result = await _userManager.CreateAsync(user,registerDTO.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Registration failed: {errors}");
            }
            await _userManager.AddToRoleAsync(user ,"Client");
        }

        public async Task ResetPasswordAsync(string email, string token, string newPassword)
        {
           var user  = await _userManager.FindByEmailAsync(email);
            if (user == null) throw new ArgumentException("invalid email");

          var result =   await _userManager.ResetPasswordAsync(user,token,newPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Reset password failed: {errors}");
            }


        }
    }
}
