using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PlazaCore.Entites;
using PlazaCore.ServiceContract;
using Shared.DTO.Rooms;

namespace Plaza.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoomController : ControllerBase
    {
        private readonly IRoomService _roomService;
        private readonly IMapper _mapper;

        public RoomController(IRoomService roomService , IMapper mapper)
        {
            this._roomService = roomService;
            this._mapper = mapper;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RoomDTO>>> GetRooms() { 
            
            var rooms = await _roomService.GetAllRoomsAsync();
            var actualRooms = _mapper.Map<IEnumerable <RoomDTO>>(rooms);
            return Ok(actualRooms);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<RoomDTO>> GetRoom(int id) { 
            var room  = await _roomService.GetRoomAsync(id);
            if (room == null) { return NotFound(); }
            return Ok(_mapper.Map<RoomDTO>(room));        
        }

        [HttpPost]
        public async Task<ActionResult> AddRoom( [FromBody] CreateRoomDTO room) {

            if (!ModelState.IsValid) {
                return BadRequest(ModelState);
            }
            var roomEntity = _mapper.Map<Room>(room);
            await _roomService.AddRoomAsync(roomEntity);
            var savedRoom = await _roomService.GetRoomAsync(roomEntity.Id);

            var createdRoomDTO = _mapper.Map<RoomDTO>(savedRoom);
            return CreatedAtAction(nameof(GetRoom), new { id = createdRoomDTO.Id }, createdRoomDTO);
            
        }
        [HttpPut("{id:int}")]   


        public async Task<ActionResult> updateRoom(int id, [FromBody] UpdateRoomDTO  roomdto) {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var existingRoom = await _roomService.GetRoomAsync(id);
            if (existingRoom == null) { return NotFound(); }

            _mapper.Map(roomdto, existingRoom);
            await _roomService.UpdateRoomAsync(existingRoom);   
            return NoContent();

        }
        [HttpDelete("{id:int}")]
        public async Task<ActionResult> deleteRoom(int id) { 
        
            var room = await _roomService.GetRoomAsync(id);
            if (room == null) { return NotFound(); };

            await _roomService.DeleteRoomAsync(id);
            return NoContent();
        }
    }
}
