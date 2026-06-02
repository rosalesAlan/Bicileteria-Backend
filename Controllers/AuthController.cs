using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;
using Bicicleteria.Backend.Data;
using Bicicleteria.Backend.DTOs;
using Bicicleteria.Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Logging;

namespace Bicicleteria.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;

        public AuthController(AppDbContext context, IConfiguration configuration, ILogger<AuthController> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        [Produces("application/json")]
        [Consumes("application/json")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Mail) || string.IsNullOrWhiteSpace(request.Password))
            {
                _logger.LogWarning("Intento de login con datos incompletos");
                return BadRequest(new { message = "Mail y contraseña son requeridos" });
            }

            var user = _context.Usuarios.FirstOrDefault(u => u.Mail == request.Mail);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                _logger.LogWarning($"Intento de login fallido para: {request.Mail}");
                return Unauthorized(new { message = "Credenciales inválidas" });
            }

            var token = GenerateJwtToken(user);
            var userDto = new UserDto
            {
                Id = user.Id,
                Nombre = user.Nombre,
                Apellido = user.Apellido,
                Mail = user.Mail,
                Rol = user.Rol
            };

            _logger.LogInformation($"Login exitoso para usuario: {user.Id} ({user.Mail})");
            return Ok(new LoginResponse { AccessToken = token, User = userDto });
        }

        private string GenerateJwtToken(User user)
        {
            var key = _configuration["JWT_KEY"];
            var issuer = _configuration["JWT_ISSER"];
            var audience = _configuration["JWT_AUDIENCE"];
            var expireMinutes = int.Parse(_configuration["JWT_EXPIRE_MINUTES"] ?? "60");

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Mail),
                new Claim(ClaimTypes.Name, $"{user.Nombre} {user.Apellido}"),
                new Claim("Rol", user.Rol)
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


        [HttpPost("register")]
        [AllowAnonymous]
        [Produces("application/json")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                _logger.LogInformation($"Intento de registro para: {request.Email}");

                // Validar contraseña fuerte
                var passwordValidation = ValidatePassword(request.Password);
                if (passwordValidation != null)
                {
                    _logger.LogWarning($"Registro fallido para {request.Email}: contraseña débil");
                    return passwordValidation;
                }

                // Email duplicado (case-insensitive)
                var emailExist = await _context.Usuarios
                    .AnyAsync(u => u.Mail.ToLower() == request.Email.Trim().ToLower());
                if (emailExist)
                {
                    _logger.LogWarning($"Intento de registro con email duplicado: {request.Email}");
                    return BadRequest(new { message = "El correo electrónico ya está en uso" });
                }

                // Teléfono duplicado
                var numeroExist = await _context.Usuarios
                    .AnyAsync(u => u.NumeroTelefono == request.NumeroTelefono.Trim());
                if (numeroExist)
                {
                    _logger.LogWarning($"Intento de registro con teléfono duplicado: {request.NumeroTelefono}");
                    return BadRequest(new { message = "Este número de teléfono ya está en uso" });
                }

                // Crear nuevo usuario
                var nuevoUsuario = new User
                {
                    Nombre = request.Nombre.Trim(),
                    Apellido = request.Apellido.Trim(),
                    Mail = request.Email.Trim().ToLower(),
                    NumeroTelefono = request.NumeroTelefono.Trim(),
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                    Rol = "cliente"
                };

                _context.Usuarios.Add(nuevoUsuario);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Usuario registrado exitosamente: {nuevoUsuario.Id} ({nuevoUsuario.Mail})");

                return Created(
                    $"/api/auth/profile/{nuevoUsuario.Id}",
                    new
                    {
                        userId = nuevoUsuario.Id,
                        email = nuevoUsuario.Mail,
                        message = "Usuario registrado exitosamente"
                    }
                );
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError($"Error de BD en registro: {ex.Message}");
                return StatusCode(500, new { message = "Error al guardar en la base de datos" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error inesperado en registro: {ex.Message}");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        private IActionResult ValidatePassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
                return BadRequest(new { message = "La contraseña debe tener mínimo 8 caracteres" });

            if (!password.Any(char.IsUpper))
                return BadRequest(new { message = "La contraseña debe contener mayúsculas (A-Z)" });

            if (!password.Any(char.IsDigit))
                return BadRequest(new { message = "La contraseña debe contener números (0-9)" });

            return null; // Válida
        }
    }
}


