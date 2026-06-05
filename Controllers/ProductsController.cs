using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Bicicleteria.Backend.Cache;
using Bicicleteria.Backend.Data;
using Bicicleteria.Backend.DTOs;
using Bicicleteria.Backend.Models;

namespace Bicicleteria.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMongoCacheService _cacheService;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(
            AppDbContext context,
            IMongoCacheService cacheService,
            ILogger<ProductsController> logger)
        {
            _context = context;
            _cacheService = cacheService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todos los productos desde caché de MongoDB (si están frescos)
        /// o desde PostgreSQL si la caché ha expirado.
        /// </summary>
        [HttpGet]
        [Produces("application/json")]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts()
        {
            try
            {
                _logger.LogInformation("GET /api/products - Obteniendo lista de productos");
                var products = await _cacheService.GetProductsAsync();
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener productos: {ex.Message}");
                return StatusCode(500, new { message = "Error al obtener productos" });
            }
        }

        /// <summary>
        /// Crear un nuevo producto e invalidar la caché.
        /// </summary>
        [HttpPost]
        [Produces("application/json")]
        [Consumes("application/json")]
        public async Task<ActionResult<Product>> CreateProduct([FromBody] Product product)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(product.Name))
                {
                    return BadRequest(new { message = "El nombre del producto es obligatorio." });
                }

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                // Invalidar caché tras crear nuevo producto
                await _cacheService.InvalidateAsync();

                _logger.LogInformation($"Producto creado: {product.Id}. Caché invalidada.");
                return CreatedAtAction(nameof(GetProducts), new { id = product.Id }, product);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al crear producto: {ex.Message}");
                return StatusCode(500, new { message = "Error al crear producto" });
            }
        }

        /// <summary>
        /// Obtiene un producto por ID.
        /// </summary>
        [HttpGet("{id}")]
        [Produces("application/json")]
        public async Task<ActionResult<Product>> GetProductById(int id)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null)
                {
                    return NotFound(new { message = "Producto no encontrado" });
                }
                return Ok(product);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener producto: {ex.Message}");
                return StatusCode(500, new { message = "Error al obtener producto" });
            }
        }

        /// <summary>
        /// Actualiza un producto e invalida la caché.
        /// </summary>
        [HttpPut("{id}")]
        [Produces("application/json")]
        [Consumes("application/json")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] Product product)
        {
            try
            {
                var existingProduct = await _context.Products.FindAsync(id);
                if (existingProduct == null)
                {
                    return NotFound(new { message = "Producto no encontrado" });
                }

                existingProduct.Name = product.Name;
                existingProduct.Description = product.Description;
                existingProduct.Price = product.Price;
                existingProduct.ImageUrl = product.ImageUrl;
                existingProduct.CategoryId = product.CategoryId;
                existingProduct.Availability = product.Availability;

                _context.Products.Update(existingProduct);
                await _context.SaveChangesAsync();

                // Invalidar caché tras actualizar
                await _cacheService.InvalidateAsync();

                _logger.LogInformation($"Producto actualizado: {id}. Caché invalidada.");
                return Ok(new { message = "Producto actualizado exitosamente", product = existingProduct });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al actualizar producto: {ex.Message}");
                return StatusCode(500, new { message = "Error al actualizar producto" });
            }
        }

        /// <summary>
        /// Elimina un producto e invalida la caché.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null)
                {
                    return NotFound(new { message = "Producto no encontrado" });
                }

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();

                // Invalidar caché tras eliminar
                await _cacheService.InvalidateAsync();

                _logger.LogInformation($"Producto eliminado: {id}. Caché invalidada.");
                return Ok(new { message = "Producto eliminado exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al eliminar producto: {ex.Message}");
                return StatusCode(500, new { message = "Error al eliminar producto" });
            }
        }

        /// <summary>
        /// Endpoint de diagnóstico para ver el estado del caché de MongoDB.
        /// </summary>
        [HttpGet("cache/status")]
        [AllowAnonymous]
        [Produces("application/json")]
        public async Task<IActionResult> GetCacheStatus()
        {
            try
            {
                var cacheDoc = await _cacheService.GetCacheDocumentAsync();
                if (cacheDoc == null)
                {
                    return Ok(new
                    {
                        status = "sin_cache",
                        message = "No hay documentos de caché en MongoDB"
                    });
                }

                return Ok(new
                {
                    status = "ok",
                    cacheId = cacheDoc.Id,
                    productosCacheados = cacheDoc.Productos.Count,
                    fechaActualizacion = cacheDoc.FechaActualizacion,
                    ttlMinutos = cacheDoc.TtlMinutos,
                    estaExpirado = cacheDoc.EstaExpirado(),
                    proximaExpiracion = cacheDoc.FechaActualizacion.AddMinutes(cacheDoc.TtlMinutos)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error obteniendo estado de caché: {ex.Message}");
                return StatusCode(500, new { message = "Error obteniendo estado de caché", error = ex.Message });
            }
        }

        /// <summary>
        /// Endpoint para invalidar manualmente la caché.
        /// </summary>
        [HttpPost("cache/invalidate")]
        [AllowAnonymous]
        public async Task<IActionResult> InvalidateCache()
        {
            try
            {
                await _cacheService.InvalidateAsync();
                return Ok(new { message = "Caché invalidada correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error invalidando caché: {ex.Message}");
                return StatusCode(500, new { message = "Error invalidando caché", error = ex.Message });
            }
        }
    }
}
