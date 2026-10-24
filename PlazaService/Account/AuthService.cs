using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Plaza.DTO.Account;
using PlazaCore.Entites;
using PlazaCore.ServiceContract.Account;
using AutoMapper;
using Shared.DTO.Account;

namespace PlazaService.Account
{
    public class AuthService : IAuthService
    {
        private const string InvalidCredentialsMessage = "Invalid email or password.";
        private const string InvalidResetMessage = "Invalid or expired password reset request.";

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IJwtTokenGenerator jwtTokenGenerator,
            IMapper mapper,
            IEmailService emailService,
            IConfiguration configuration)
        {
            this._userManager = userManager;
            this._signInManager = signInManager;
            this._jwtTokenGenerator = jwtTokenGenerator;
            this._mapper = mapper;
            this._emailService = emailService;
            this._configuration = configuration;
        }

        public async Task ForgotPasswordAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return;

            var frontendBaseUrl = _configuration["FrontendBaseUrl"];
            if (string.IsNullOrWhiteSpace(frontendBaseUrl))
                throw new InvalidOperationException("FrontendBaseUrl is not configured.");

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetLink = $"{frontendBaseUrl.TrimEnd('/')}/reset-password?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(token)}";
            await _emailService.SendEmailAsync(email, "Reset Password",
                $"Click <a href='{resetLink}'>here</a> to reset your password.",null);
        }

        public async Task<LoginResultDTO> LoginAsync(LoginDTO loginDTO)
        {
            var user = await _userManager.FindByEmailAsync(loginDTO.Email);
            if (user == null)
                return new LoginResultDTO { Success = false, Message = InvalidCredentialsMessage };

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDTO.Password, lockoutOnFailure: true);

            if (result.IsLockedOut)
                return new LoginResultDTO { Success = false, Message = "Account is temporarily locked due to multiple failed login attempts. Please try again later." };

            if (!result.Succeeded)
                return new LoginResultDTO { Success = false, Message = InvalidCredentialsMessage };

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

        public async Task ResetPasswordAsync(string email, string token, string newPassword )
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                throw new InvalidOperationException(InvalidResetMessage);

            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
            if (!result.Succeeded)
            {
                if (result.Errors.Any(e => e.Code == nameof(IdentityErrorDescriber.InvalidToken)))
                    throw new InvalidOperationException(InvalidResetMessage);

                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Reset password failed: {errors}");
            }

            // A successful reset should also clear any active lockout so the user can sign in immediately.
            await _userManager.ResetAccessFailedCountAsync(user);
            await _userManager.SetLockoutEndDateAsync(user, null);
        }
    }
}
