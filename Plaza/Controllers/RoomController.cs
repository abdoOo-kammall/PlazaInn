﻿using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PlazaCore.Entites;
using PlazaCore.ServiceContract;
using Shared.DTO.Rooms;
using Shared.Security;

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

        [HttpGet("encodedId")]
        public async Task<ActionResult<RoomDTO>> GetRoom(string encodedId) {
            int id;
            try { 
                id = IdEncoder.DecodeId(encodedId);
            } catch (Exception ex) {
                return BadRequest("Invalid Room Id"); 
            }
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
            var encodedId = IdEncoder.EncodeId(savedRoom.Id);
            return CreatedAtAction(nameof(GetRoom), new { encodedId }, createdRoomDTO);
            
        }
        [HttpPut("encodedId")]   


        public async Task<ActionResult> updateRoom(string encodedId, [FromBody] UpdateRoomDTO  roomdto) {
            int id;
            try
            {
                id = IdEncoder.DecodeId(encodedId); 
            }
            catch (Exception ex) {
                return BadRequest("Invalid Room ID");
            }
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
        [HttpDelete("encodedId")]
        public async Task<ActionResult> deleteRoom(string encodedId) {
            int id;
            try
            {
                id = IdEncoder.DecodeId(encodedId);
            }
            catch (Exception ex)
            {
                return BadRequest("Invalid room ID");
            }

            var room = await _roomService.GetRoomAsync(id);
            if (room == null) { return NotFound(); };

            await _roomService.DeleteRoomAsync(id);
            return NoContent();
        }
    }
}
