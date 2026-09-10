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
        /// o desde PostgreSQL si la caché ha expirado. Endpoint público.
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
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
        /// Obtiene un producto por ID desde caché. Endpoint público.
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        [Produces("application/json")]
        public async Task<ActionResult<ProductDto>> GetProductById(int id)
        {
            try
            {
                var products = await _cacheService.GetProductsAsync();
                var product = products.FirstOrDefault(p => p.Id == id);

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
        /// Crear un nuevo producto e invalidar la caché. Solo para Administradores.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [Produces("application/json")]
        [Consumes("application/json")]
        public async Task<ActionResult<Product>> CreateProduct([FromBody] CreateProductDto CreateProductDto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(CreateProductDto.Name))
                {
                    return BadRequest(new { message = "El nombre del producto es obligatorio." });
                }

                var product = new Product
                {
                    Name = CreateProductDto.Name,
                    Description = CreateProductDto.Description,
                    Price = CreateProductDto.Price,
                    ImageUrl = CreateProductDto.ImageUrl,
                    CategoryId = CreateProductDto.CategoryId,
                    Availability = CreateProductDto.Availability
                };

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                // Invalidar caché tras crear nuevo producto
                await _cacheService.InvalidateAsync();

                _logger.LogInformation($"Producto creado: {product.Id} - {product.Name}. Caché invalidada.");

                return Ok(product); // retorna
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al crear producto: {ex.Message}");
                return StatusCode(500, new { message = "Error al crear producto" });
            }
        }

        /// <summary>
        /// Actualiza un producto e invalida la caché. Solo para Administradores.
        /// </summary>
        [HttpPut("{id}")]
        [Authorize]
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
        /// Elimina un producto e invalida la caché. Solo para Administradores.
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
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
        /// Obtiene el documento de caché completo de MongoDB. Solo para Administradores.
        /// </summary>
        [HttpGet("cached")]
        [Authorize(Roles = "Admin")]
        [Produces("application/json")]
        public async Task<IActionResult> GetCachedDocument()
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
                    id = cacheDoc.Id,
                    productos = cacheDoc.Productos,
                    productosCacheados = cacheDoc.Productos.Count,
                    fechaActualizacion = cacheDoc.FechaActualizacion,
                    ttlMinutos = cacheDoc.TtlMinutos,
                    estaExpirado = cacheDoc.EstaExpirado(),
                    proximaExpiracion = cacheDoc.FechaActualizacion.AddMinutes(cacheDoc.TtlMinutos)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error obteniendo documento de caché: {ex.Message}");
                return StatusCode(500, new { message = "Error obteniendo documento de caché", error = ex.Message });
            }
        }

        /// <summary>
        /// Fuerza la recarga del caché desde la base de datos. Solo para Administradores.
        /// </summary>
        [HttpPost("sync-cache")]
        [Authorize(Roles = "Admin")]
        [Produces("application/json")]
        public async Task<IActionResult> SyncCache()
        {
            try
            {
                await _cacheService.InvalidateAsync();
                var products = await _cacheService.GetProductsAsync();

                var cacheDoc = await _cacheService.GetCacheDocumentAsync();
                return Ok(new 
                { 
                    message = "Caché recargada correctamente",
                    productosCacheados = products.Count(),
                    cacheDocument = cacheDoc
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sincronizando caché: {ex.Message}");
                return StatusCode(500, new { message = "Error sincronizando caché", error = ex.Message });
            }
        }

        [HttpGet("debug")]
        [Authorize]
        public IActionResult Debug()
        {
            var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
            var isAuthenticated = User.Identity?.IsAuthenticated ?? false;
            var name = User.Identity?.Name;
            var roles = User.FindAll("role").Select(c => c.Value).ToList();

            return Ok(new { isAuthenticated, name, roles, claims });
        }
    }
}
