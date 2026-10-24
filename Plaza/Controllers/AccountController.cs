using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Plaza.DTO.Account;
using Plaza.Extensions;
using PlazaCore.ServiceContract.Account;
using Shared.DTO.Account;

namespace Plaza.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private const string ForgotPasswordMessage = "If an account with that email exists, a password reset link has been sent.";

        private readonly IAuthService _authService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IAuthService authService, ILogger<AccountController> logger)
        {
            this._authService = authService;
            this._logger = logger;
        }

        [HttpPost("login")]
        [EnableRateLimiting(RateLimitingExtensions.AuthPolicy)]
        public async Task<IActionResult> login(LoginDTO loginDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.LoginAsync(loginDTO);
            if (!result.Success)
                return Unauthorized(new { message = result.Message });

            return Ok(new
            {
                message = result.Message,
                token = result.Token
            });
        }

        [HttpPost("register")]
        [EnableRateLimiting(RateLimitingExtensions.AuthPolicy)]
        public async Task<IActionResult> Register([FromBody] RegisterDTO registerDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _authService.RegisterAsync(registerDTO);
                return Ok(new { message = "Registration successful." });
            }
            catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("forgot-password")]
        [EnableRateLimiting(RateLimitingExtensions.AuthPolicy)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _authService.ForgotPasswordAsync(dto.Email);
            }
            catch (Exception ex)
            {
                // Same response either way so callers cannot probe which emails are registered.
                _logger.LogError(ex, "Failed to process forgot-password request.");
            }

            return Ok(new { message = ForgotPasswordMessage });
        }

        [HttpPost("reset-password")]
        [EnableRateLimiting(RateLimitingExtensions.AuthPolicy)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                dto.Token = Uri.UnescapeDataString(dto.Token);

                await _authService.ResetPasswordAsync(dto.Email, dto.Token, dto.NewPassword);
                return Ok(new { message = "Password reset successful." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

    }
}
