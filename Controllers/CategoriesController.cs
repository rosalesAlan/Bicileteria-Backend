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
    public class CategoriesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CategoriesController> _logger;

        public CategoriesController(AppDbContext context, ILogger<CategoriesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todas las categorías. Endpoint público.
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        [Produces("application/json")]
        public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
        {
            try
            {
                _logger.LogInformation("GET /api/categories - Obteniendo lista de categorías");
                var categories = await _context.Categories
                    .OrderBy(c => c.Name)
                    .ToListAsync();

                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener categorías: {ex.Message}");
                return StatusCode(500, new { message = "Error al obtener categorías" });
            }
        }

        /// <summary>
        /// Obtiene una categoría por ID. Endpoint público.
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        [Produces("application/json")]
        public async Task<ActionResult<Category>> GetCategoryById(int id)
        {
            try
            {
                var category = await _context.Categories.FindAsync(id);
                if (category == null)
                {
                    return NotFound(new { message = "Categoría no encontrada" });
                }
                return Ok(category);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener categoría: {ex.Message}");
                return StatusCode(500, new { message = "Error al obtener categoría" });
            }
        }

        /// <summary>
        /// Crea una nueva categoría. Solo para administradores.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [Produces("application/json")]
        [Consumes("application/json")]
        public async Task<ActionResult<Category>> CreateCategory([FromBody] Category category)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(category.Name))
                {
                    return BadRequest(new { message = "El nombre de la categoría es obligatorio." });
                }

                // Verificar si la categoría ya existe
                var existingCategory = await _context.Categories
                    .FirstOrDefaultAsync(c => c.Name.ToLower() == category.Name.ToLower());

                if (existingCategory != null)
                {
                    return BadRequest(new { message = "Una categoría con este nombre ya existe." });
                }

                _context.Categories.Add(category);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Categoría creada: {category.Id} - {category.Name}");
                return CreatedAtAction(nameof(GetCategoryById), new { id = category.Id }, category);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al crear categoría: {ex.Message}");
                return StatusCode(500, new { message = "Error al crear categoría" });
            }
        }

        /// <summary>
        /// Actualiza una categoría por ID. Solo para administradores.
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        [Produces("application/json")]
        [Consumes("application/json")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] Category category)
        {
            try
            {
                var existingCategory = await _context.Categories.FindAsync(id);
                if (existingCategory == null)
                {
                    return NotFound(new { message = "Categoría no encontrada" });
                }

                if (string.IsNullOrWhiteSpace(category.Name))
                {
                    return BadRequest(new { message = "El nombre de la categoría es obligatorio." });
                }

                // Verificar si ya existe otra categoría con el mismo nombre
                var duplicateCategory = await _context.Categories
                    .FirstOrDefaultAsync(c => c.Name.ToLower() == category.Name.ToLower() && c.Id != id);

                if (duplicateCategory != null)
                {
                    return BadRequest(new { message = "Una categoría con este nombre ya existe." });
                }

                existingCategory.Name = category.Name;
                _context.Categories.Update(existingCategory);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Categoría actualizada: {id} - {category.Name}");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al actualizar categoría: {ex.Message}");
                return StatusCode(500, new { message = "Error al actualizar categoría" });
            }
        }

        /// <summary>
        /// Elimina una categoría por ID. Solo para administradores.
        /// Solo permite eliminar si no tiene productos asociados.
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            try
            {
                var category = await _context.Categories.FindAsync(id);
                if (category == null)
                {
                    return NotFound(new { message = "Categoría no encontrada" });
                }

                // Verificar si la categoría tiene productos asociados
                var productsWithCategory = await _context.Products
                    .Where(p => p.CategoryId == id)
                    .AnyAsync();

                if (productsWithCategory)
                {
                    return BadRequest(new 
                    { 
                        message = "No se puede eliminar la categoría porque tiene productos asociados. Reasigne o elimine los productos primero." 
                    });
                }

                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Categoría eliminada: {id} - {category.Name}");
                return Ok(new { message = "Categoría eliminada exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al eliminar categoría: {ex.Message}");
                return StatusCode(500, new { message = "Error al eliminar categoría" });
            }
        }
    }
}
