using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Bicicleteria.Backend.Data;
using Bicicleteria.Backend.Models;

namespace Bicicleteria.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            var products = await _context.Productos.ToListAsync();
            return Ok(products);
        }

        [HttpPost]
        public async Task<ActionResult<Product>> CreateProduct([FromBody] Product product)
        {
            if (string.IsNullOrWhiteSpace(product.Name))
            {
                return BadRequest(new { message = "El nombre del producto es obligatorio." });
            }

            _context.Productos.Add(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProducts), new { id = product.Id }, product);
        }
    }
}
