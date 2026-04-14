using Hotel_Management_System.Data;
using Hotel_Management_System.DTOs;
using Hotel_Management_System.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hotel_Management_System.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoomController : ControllerBase
    {

        private readonly AppDbContext _context;

        public RoomController(AppDbContext context)
        {
            _context = context;
        }

        // ✅ View all rooms — Any logged in user
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAllRooms()
        {
            var rooms = await _context.Rooms
                            .Select(r => new RoomResponseDto
                            {
                                Id = r.Id,
                                RoomNumber = r.RoomNumber,
                                Type = r.Type,
                                Price = r.Price,
                                IsAvailable = r.IsAvailable
                            })
                            .ToListAsync();

            return Ok(rooms);
        }

        // ✅ View available rooms only — Any logged in user
        [Authorize]
        [HttpGet("available")]
        public async Task<IActionResult> GetAvailableRooms()
        {
            var rooms = await _context.Rooms
                            .Where(r => r.IsAvailable == true)
                            .Select(r => new RoomResponseDto
                            {
                                Id = r.Id,
                                RoomNumber = r.RoomNumber,
                                Type = r.Type,
                                Price = r.Price,
                                IsAvailable = r.IsAvailable
                            })
                            .ToListAsync();

            return Ok(rooms);
        }

        // ✅ Add new room — Admin only
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> AddRoom([FromBody] CreateRoomDto request)
        {
            // Check room number already exists
            bool exists = await _context.Rooms
                              .AnyAsync(r => r.RoomNumber == request.RoomNumber);

            if (exists)
                return BadRequest("Room number already exists!");

            var room = new Room
            {
                RoomNumber = request.RoomNumber,
                Type = request.Type,
                Price = request.Price,
                IsAvailable = true
            };

            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();

            return Ok($"Room {request.RoomNumber} added successfully!");
        }

        // ✅ Update room — Admin only
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRoom(int id,
                                            [FromBody] CreateRoomDto request)
        {
            var room = await _context.Rooms.FindAsync(id);

            if (room == null)
                return NotFound("Room not found!");

            room.RoomNumber = request.RoomNumber;
            room.Type = request.Type;
            room.Price = request.Price;

            await _context.SaveChangesAsync();

            return Ok($"Room {id} updated successfully!");
        }

        // ✅ Delete room — Admin only
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRoom(int id)
        {
            var room = await _context.Rooms.FindAsync(id);

            if (room == null)
                return NotFound("Room not found!");

            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();

            return Ok($"Room {id} deleted successfully!");
        }
    }
}
