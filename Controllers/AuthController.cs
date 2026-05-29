using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;
using Bicicleteria.Backend.Data;
using Bicicleteria.Backend.DTOs;
using Bicicleteria.Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Bicicleteria.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        [Produces("application/json")]
        [Consumes("application/json")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Mail) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { message = "Mail y contraseña son requeridos" });
            }

            var user = _context.Usuarios.FirstOrDefault(u => u.Mail == request.Mail);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return Unauthorized(new { message = "Credenciales inválidas" });
            }

            var token = GenerateJwtToken(user);
            var userDto = new UserDto
            {
                Id = user.Id,
                Nombre = user.Nombre,
                Apellido = user.Apellido,
                Mail = user.Mail,
                Tipo = user.Tipo
            };

            return Ok(new LoginResponse { AccessToken = token, User = userDto });
        }

        private string GenerateJwtToken(User user)
        {
            var key = _configuration["Jwt:Key"];
            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];
            var expireMinutes = int.Parse(_configuration["Jwt:ExpireMinutes"] ?? "60");

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Mail),
                new Claim(ClaimTypes.Name, $"{user.Nombre} {user.Apellido}"),
                new Claim("Tipo", user.Tipo)
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expireMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
