using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Plaza.DTO.Account;
using PlazaCore.ServiceContract.Account;

namespace Plaza.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AccountController(IAuthService authService)
        {
            this._authService = authService;
        }
        [HttpPost("login")]
        
        public async Task< IActionResult> login(LoginDTO loginDTO) {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _authService.LoginAsync(loginDTO);
                if (result == null)
                    return Unauthorized(new { message = "Invalid email or password." });

                return Ok(new
                {
                    message = result.Message,
                    token = result.Token
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        
        }

        [HttpPost("register")]
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
            catch (Exception ex)
            {
               
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
