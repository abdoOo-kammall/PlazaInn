using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PlazaCore.Entites;
using PlazaCore.ServiceContract;
using Shared.DTO.User;

namespace Plaza.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public UserController( IUserService userService , IMapper mapper)
        {
            this._userService = userService;
            this._mapper = mapper;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDTO>>> getAllUsers()
        {
            var users = await _userService.getAllUsersAsync();
            var userDtos = new List<UserDTO>();
            foreach (var user in users) { 
                var actualUser = _mapper.Map<UserDTO>(user);
                actualUser.Role = await _userService.getUserRoleAsync(user)?? "No Role";
                userDtos.Add(actualUser);
            }
            return Ok(userDtos);
        }
        [HttpGet("{userId}")]
        public async Task<ActionResult<UserDTO>> getUserById(string userId) {

            var user = await _userService.getUserByIdAsync(userId);
            if (user == null)
                return NotFound();

            var dto = _mapper.Map<UserDTO>(user);
            dto.Role = await _userService.getUserRoleAsync(user) ?? "No Role";

            return Ok(dto);
        }
        [HttpPost]
        public async Task<ActionResult> addUser([FromBody] CreateUserDTO dTO) {

            if (dTO.Password != dTO.PasswordConfirmed)
                {
                return BadRequest("Password and confirmation do not match.");

            }
            var user = _mapper.Map<ApplicationUser>(dTO);
            var result = await _userService.addUserAsync(user,dTO.Password ,dTO.Role);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok("User created successfully.");

        }
        [HttpPut("{id}")]
        public async Task<ActionResult> updateUser(string id, [FromBody] updateUserDTO dto)
        {
            try
            {
                var result = await _userService.updateUserAsync(id, dto);
                if (!result.Succeeded)
                    return BadRequest(result.Errors);

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpDelete("{id}")]
        public async Task<ActionResult> deleteUser(string id)
        {
            try
            {
                var result = await _userService.deleteUserAsync(id);
                if (!result.Succeeded)
                    return BadRequest(result.Errors);

                return NoContent();
            }
            catch (Exception ex) { 
                return BadRequest(ex.Message);
            }
        }


    }
}
