using Hotel_Management_System.Model;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Hotel_Management_System.Services
{
    public class JWTService
    {
        private readonly IConfiguration _config;

        public JWTService(IConfiguration config)
        {
            _config = config;
        }

        public string GenerateToken(User user)
        {
            var jwtSettings = _config.GetSection("JwtSettings");
            var key = new SymmetricSecurityKey(
                          Encoding.UTF8.GetBytes(jwtSettings["Key"]!));

            // 📝 Information written on the key card
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier,
                          user.id.ToString()),       // Guest ID
                new Claim(ClaimTypes.Name,
                          user.Username),            // Guest Name
                new Claim(ClaimTypes.Role,
                          user.Role),                // Admin or Guest
                new Claim(JwtRegisteredClaimNames.Jti,
                          Guid.NewGuid().ToString()) // Unique card ID
            };

            // 🔐 Stamp the card
            var credentials = new SigningCredentials(
                                  key, SecurityAlgorithms.HmacSha256);

            // 🪪 Create the key card
            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(
                             Convert.ToDouble(jwtSettings["ExpiryMinutes"])),
                signingCredentials: credentials
            );

            // 🎴 Convert card to string and return
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // ✅ Helper — get logged in user ID from token
        public int GetUserIdFromToken(ClaimsPrincipal user)
        {
            var claim = user.FindFirst(ClaimTypes.NameIdentifier);
            return claim != null ? Convert.ToInt32(claim.Value) : 0;
        }
    }
}
