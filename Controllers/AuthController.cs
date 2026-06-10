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
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                _logger.LogWarning("Intento de login con datos incompletos");
                return BadRequest(new { message = "Email y contraseña son requeridos" });
            }
            

            var user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                _logger.LogWarning($"Intento de login fallido para: {request.Email}");
                return Unauthorized(new { message = "Credenciales inválidas" });
            }

            var token = GenerateJwtToken(user);
            var userDto = new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                RoleName = user.Role.Name
            };

            _logger.LogInformation($"Login exitoso para usuario: {user.Id} ({user.Email})");
            return Ok(new LoginResponse { AccessToken = token, User = userDto });
        }

        private string GenerateJwtToken(User user)
        {
            var key = _configuration["JWT_KEY"];
            var issuer = _configuration["JWT_ISSUER"];
            var audience = _configuration["JWT_AUDIENCE"];
            var expireMinutes = int.Parse(_configuration["JWT_EXPIRE_MINUTES"] ?? "60");

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                new Claim("role", user.Role.Name)
            };
            Console.WriteLine($"Generando JWT para usuario {user.Id} ({user.Email}), rol: {user.Role.Name}");

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
                var emailExists = await _context.Users
                    .AnyAsync(u => u.Email.ToLower() == request.Email.Trim().ToLower());
                if (emailExists)
                {
                    _logger.LogWarning($"Intento de registro con email duplicado: {request.Email}");
                    return BadRequest(new { message = "El correo electrónico ya está en uso" });
                }

                // Teléfono duplicado
                var phoneExists = await _context.Users
                    .AnyAsync(u => u.PhoneNumber == request.PhoneNumber.Trim());
                if (phoneExists)
                {
                    _logger.LogWarning($"Intento de registro con teléfono duplicado: {request.PhoneNumber}");
                    return BadRequest(new { message = "Este número de teléfono ya está en uso" });
                }

                // Obtener rol por defecto (Customer)
                var customerRole = await _context.Roles.FirstOrDefaultAsync(r => r.Id == 4); // name = cliente; id = 4
                if (customerRole == null)
                {
                    _logger.LogError("No se encontró el rol 'Cliente' en la base de datos");
                    return StatusCode(500, new { message = "Error al asignar rol de usuario" });
                }

                // Crear nuevo usuario
                var newUser = new User
                {
                    FirstName = request.FirstName.Trim(),
                    LastName = request.LastName.Trim(),
                    Email = request.Email.Trim().ToLower(),
                    PhoneNumber = request.PhoneNumber.Trim(),
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                    RoleId = customerRole.Id
                };

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Usuario registrado exitosamente: {newUser.Id} ({newUser.Email})");

                return Created(
                    $"/api/auth/profile/{newUser.Id}",
                    new
                    {
                        userId = newUser.Id,
                        email = newUser.Email,
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


