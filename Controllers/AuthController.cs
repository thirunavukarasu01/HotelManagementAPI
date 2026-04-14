using Hotel_Management_System.Data;
using Hotel_Management_System.DTOs;
using Hotel_Management_System.Model;
using Hotel_Management_System.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace Hotel_Management_System.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {

        private readonly AppDbContext _context;
        private readonly JWTService _jwtService;

        public AuthController(AppDbContext context, JWTService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        // ✅ SIGNUP
        [HttpPost("signup")]
        public async Task<IActionResult> SignUp([FromBody] AuthRequestDto request)
        {
            // 1. Check empty fields
            if (string.IsNullOrWhiteSpace(request.Username) ||
                string.IsNullOrWhiteSpace(request.Password))
                return BadRequest("Username and Password are required");

            // 2. Check if username already exists
            bool userExists = await _context.Users
                                .AnyAsync(u => u.Username == request.Username);

            if (userExists)
                return BadRequest("Username already taken!");

            // 3. Hash the password
            string hashedPassword = BCrypt.Net.BCrypt
                                        .HashPassword(request.Password);

            // 4. Create new user
            var newUser = new User
            {
                Username = request.Username,
                PasswordHash = hashedPassword,
                Role = "Guest" // default role
            };

            // 5. Save to database
            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return Ok("Signup successful! Please login.");
        }


        // ✅ LOGIN
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AuthRequestDto request)
        {
            // 1. Check empty fields
            if (string.IsNullOrWhiteSpace(request.Username) ||
                string.IsNullOrWhiteSpace(request.Password))
                return BadRequest("Username and Password are required");

            // 2. Find user in database
            var user = await _context.Users
                            .FirstOrDefaultAsync(u =>
                                u.Username == request.Username);

            // 3. User not found
            if (user == null)
                return Unauthorized("Invalid username or password");

            // 4. Verify password
            bool isPasswordCorrect = BCrypt.Net.BCrypt
                                        .Verify(request.Password,
                                                user.PasswordHash);

            // 5. Wrong password
            if (!isPasswordCorrect)
                return Unauthorized("Invalid username or password");

            // 6. ✅ All correct — Generate token
            var token = _jwtService.GenerateToken(user);

            return Ok(new AuthResponseDto
            {
                Token = token,
                Username = user.Username,
                Role = user.Role,
                Expires = DateTime.UtcNow.AddMinutes(60)
            });
        }


        // ✅ CREATE ADMIN (one time use)
        [HttpPost("create-admin")]
        public async Task<IActionResult> CreateAdmin([FromBody] AuthRequestDto request)
        {
            bool userExists = await _context.Users
                                .AnyAsync(u => u.Username == request.Username);

            if (userExists)
                return BadRequest("Admin already exists!");

            var admin = new User
            {
                Username = request.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = "Admin" // ✅ Admin role
            };

            _context.Users.Add(admin);
            await _context.SaveChangesAsync();

            return Ok("Admin created successfully!");
        }


    }
}
