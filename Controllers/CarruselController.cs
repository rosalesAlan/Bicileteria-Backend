using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Bicicleteria.Backend.Data;
using Bicicleteria.Backend.Models;

namespace Bicicleteria.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CarruselController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CarruselController> _logger;

        public CarruselController(AppDbContext context, ILogger<CarruselController> _logger)
        {
            _context = context;
            this._logger = _logger;
        }

        /// <summary>
        /// Obtiene todos los slides del carrusel ordenados por rango (Order).
        /// Incluye la categoría relacionada. Endpoint público.
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        [Produces("application/json")]
        public async Task<ActionResult<IEnumerable<object>>> GetCarouselItems()
        {
            try
            {
                _logger.LogInformation("GET /api/carrusel - Obteniendo slides del carrusel");

                var carouselItems = await _context.CarouselItems
                    .Include(c => c.Category)
                    .OrderBy(c => c.Range   )
                    .ToListAsync();

                var result = carouselItems.Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.Range,
                    rango = c.Range,
                    category = c.Category != null ? new { c.Category.Id, c.Category.Name } : null,
                    categoryId = c.CategoryId
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener slides del carrusel: {ex.Message}");
                return StatusCode(500, new { message = "Error al obtener slides del carrusel" });
            }
        }

        /// <summary>
        /// Obtiene un slide del carrusel por ID. Incluye la categoría relacionada. Endpoint público.
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        [Produces("application/json")]
        public async Task<ActionResult<object>> GetCarouselItemById(int id)
        {
            try
            {
                var carouselItem = await _context.CarouselItems
                    .Include(c => c.Category)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (carouselItem == null)
                {
                    return NotFound(new { message = "Slide del carrusel no encontrado" });
                }

                var result = new
                {
                    carouselItem.Id,
                    carouselItem.Name,
                    carouselItem.Range,
                    rango = carouselItem.Range,
                    category = carouselItem.Category != null ? new { carouselItem.Category.Id, carouselItem.Category.Name } : null,
                    categoryId = carouselItem.CategoryId
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener slide del carrusel: {ex.Message}");
                return StatusCode(500, new { message = "Error al obtener slide del carrusel" });
            }
        }

        /// <summary>
        /// Crea un nuevo slide del carrusel. Solo para administradores.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [Produces("application/json")]
        [Consumes("application/json")]
        public async Task<ActionResult<object>> CreateCarouselItem([FromBody] CarouselItem carouselItem)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(carouselItem.Name))
                {
                    return BadRequest(new { message = "El nombre del slide es obligatorio." });
                }

                // Validar que la categoría exista si se proporciona
                if (carouselItem.CategoryId.HasValue)
                {
                    var categoryExists = await _context.Categories
                        .AnyAsync(c => c.Id == carouselItem.CategoryId.Value);

                    if (!categoryExists)
                    {
                        return BadRequest(new { message = "La categoría especificada no existe." });
                    }
                }

                _context.CarouselItems.Add(carouselItem);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Slide del carrusel creado: {carouselItem.Id} - {carouselItem.Name}");

                var result = new
                {
                    carouselItem.Id,
                    carouselItem.Name,
                    carouselItem.Range,
                    rango = carouselItem.Range,
                    categoryId = carouselItem.CategoryId,
                    category = carouselItem.Category != null ? new { carouselItem.Category.Id, carouselItem.Category.Name } : null
                };

                return CreatedAtAction(nameof(GetCarouselItemById), new { id = carouselItem.Id }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al crear slide del carrusel: {ex.Message}");
                return StatusCode(500, new { message = "Error al crear slide del carrusel" });
            }
        }

        /// <summary>
        /// <summary>
        /// Actualiza un slide del carrusel. Solo para administradores.
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        [Produces("application/json")]
        [Consumes("application/json")]
        public async Task<IActionResult> UpdateCarouselItem(int id, [FromBody] CarouselItem carouselItem)
        {
            try
            {
                var existingItem = await _context.CarouselItems.FindAsync(id);
                if (existingItem == null)
                {
                    return NotFound(new { message = "Slide del carrusel no encontrado" });
                }

                if (string.IsNullOrWhiteSpace(carouselItem.Name))
                {
                    return BadRequest(new { message = "El nombre del slide es obligatorio." });
                }

                // Validar que la categoría exista si se proporciona
                if (carouselItem.CategoryId.HasValue)
                {
                    var categoryExists = await _context.Categories
                        .AnyAsync(c => c.Id == carouselItem.CategoryId.Value);

                    if (!categoryExists)
                    {
                        return BadRequest(new { message = "La categoría especificada no existe." });
                    }
                }

                existingItem.Name = carouselItem.Name;
                existingItem.Range = carouselItem.Range;
                existingItem.CategoryId = carouselItem.CategoryId;

                _context.CarouselItems.Update(existingItem);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Slide del carrusel actualizado: {id} - {carouselItem.Name}");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al actualizar slide del carrusel: {ex.Message}");
                return StatusCode(500, new { message = "Error al actualizar slide del carrusel" });
            }
        }

        /// <summary>
        /// Elimina un slide del carrusel. Solo para administradores.
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCarouselItem(int id)
        {
            try
            {
                var carouselItem = await _context.CarouselItems.FindAsync(id);
                if (carouselItem == null)
                {
                    return NotFound(new { message = "Slide del carrusel no encontrado" });
                }

                _context.CarouselItems.Remove(carouselItem);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Slide del carrusel eliminado: {id} - {carouselItem.Name}");
                return Ok(new { message = "Slide del carrusel eliminado exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al eliminar slide del carrusel: {ex.Message}");
                return StatusCode(500, new { message = "Error al eliminar slide del carrusel" });
            }
        }
    }
}
