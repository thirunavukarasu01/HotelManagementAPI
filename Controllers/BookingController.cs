using Hotel_Management_System.Data;
using Hotel_Management_System.DTOs;
using Hotel_Management_System.Model;
using Hotel_Management_System.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hotel_Management_System.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly JWTService _jwtService;

        public BookingController(AppDbContext context, JWTService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        // ✅ Book a room — Any logged in guest
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> BookRoom([FromBody] CreateBookingDto request)
        {
            // 1. Get logged in user ID from token
            int userId = _jwtService.GetUserIdFromToken(User);

            if (userId == 0)
                return Unauthorized("Invalid token");

            // 2. Check room exists
            var room = await _context.Rooms.FindAsync(request.RoomId);

            if (room == null)
                return NotFound("Room not found!");

            // 3. Check room is available
            if (!room.IsAvailable)
                return BadRequest("Room is not available!");

            // 4. Validate dates
            if (request.CheckIn >= request.CheckOut)
                return BadRequest("CheckOut must be after CheckIn!");

            if (request.CheckIn < DateTime.UtcNow.Date)
                return BadRequest("CheckIn cannot be in the past!");

            // 5. Create booking
            var booking = new Booking
            {
                UserId = userId,
                RoomId = request.RoomId,
                CheckIn = request.CheckIn,
                CheckOut = request.CheckOut,
                Status = "Confirmed"
            };

            // 6. Mark room as unavailable
            room.IsAvailable = false;

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            return Ok("Room booked successfully!");
        }

        // ✅ My bookings — logged in guest sees own bookings
        [Authorize]
        [HttpGet("my-bookings")]
        public async Task<IActionResult> MyBookings()
        {
            int userId = _jwtService.GetUserIdFromToken(User);

            var bookings = await _context.Bookings
                .Where(b => b.UserId == userId)
                .Include(b => b.Room)
                .Select(b => new BookingResponseDto
                {
                    Id = b.Id,
                    RoomNumber = b.Room.RoomNumber,
                    RoomType = b.Room.Type,
                    CheckIn = b.CheckIn,
                    CheckOut = b.CheckOut,
                    Status = b.Status,
                    BookedAt = b.BookedAt
                })
                .ToListAsync();

            return Ok(bookings);
        }

        // ✅ Cancel booking — logged in guest
        [Authorize]
        [HttpPut("cancel/{id}")]
        public async Task<IActionResult> CancelBooking(int id)
        {
            int userId = _jwtService.GetUserIdFromToken(User);

            // Find booking that belongs to this user
            var booking = await _context.Bookings
                              .Include(b => b.Room)
                              .FirstOrDefaultAsync(b =>
                                    b.Id == id && b.UserId == userId);

            if (booking == null)
                return NotFound("Booking not found!");

            if (booking.Status == "Cancelled")
                return BadRequest("Booking already cancelled!");

            // Cancel booking + make room available again
            booking.Status = "Cancelled";
            booking.Room.IsAvailable = true;

            await _context.SaveChangesAsync();

            return Ok("Booking cancelled successfully!");
        }

        // ✅ All bookings — Admin only
        [Authorize(Roles = "Admin")]
        [HttpGet("all")]
        public async Task<IActionResult> AllBookings()
        {
            var bookings = await _context.Bookings
                .Include(b => b.Room)
                .Include(b => b.User)
                .Select(b => new
                {
                    b.Id,
                    GuestName = b.User.Username,
                    RoomNumber = b.Room.RoomNumber,
                    RoomType = b.Room.Type,
                    b.CheckIn,
                    b.CheckOut,
                    b.Status,
                    b.BookedAt
                })
                .ToListAsync();

            return Ok(bookings);
        }
    }
}
