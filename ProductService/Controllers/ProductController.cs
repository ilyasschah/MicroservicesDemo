using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ProductService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly ProductDbContext _context;

        public ProductController(ProductDbContext context)
        {
            _context = context;
        }
        public class ProductCreateDto
        {
            public string Name { get; set; }
            public decimal Price { get; set; }
            public int CategoryId { get; set; }
            public int Stock { get; set; }
        }
        // GET: /Product
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> Get()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Select(static p => new {
                    p.Id,
                    p.Name,
                    p.Price,
                    p.CategoryId,
                    CategoryName = p.Category.Name
                })
                .ToListAsync();

            return Ok(products);
        }

        // POST: /Product
        [HttpPost]
        public async Task<ActionResult<Product>> Post([FromBody] ProductCreateDto dto)
        {
            // Simple validation
            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest(new { error = "Product name is required" });
            if (dto.Price < 0)
                return BadRequest(new { error = "Price cannot be negative" });

            // Validate CategoryId exists
            var category = await _context.Categories.FindAsync(dto.CategoryId);
            if (category == null)
                return BadRequest(new { error = "Invalid CategoryId" });

            // Check for duplicate product (same name and category)
            var duplicate = await _context.Products
                .AnyAsync(p => p.Name == dto.Name && p.CategoryId == dto.CategoryId);

            if (duplicate)
                return Conflict(new { error = "A product with the same name and category already exists." });

            var product = new Product
            {
                Name = dto.Name,
                Price = dto.Price,
                CategoryId = dto.CategoryId
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // Create inventory for the new product with user-specified stock
            _context.Inventories.Add(new Inventory
            {
                ProductId = product.Id,
                Stock = dto.Stock // Use the stock value from the DTO
            });
            await _context.SaveChangesAsync();

            // Optionally include the category in the returned product
            product.Category = await _context.Categories.FindAsync(dto.CategoryId);

            return CreatedAtAction(nameof(Get), new { id = product.Id }, product);
        }
        // PUT: /Product/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Product product)
        {
            if (id != product.Id)
                return BadRequest(new { error = "ID mismatch" });

            if (string.IsNullOrWhiteSpace(product.Name))
                return BadRequest(new { error = "Product name is required" });
            if (product.Price < 0)
                return BadRequest(new { error = "Price cannot be negative" });

            // Validate CategoryId exists
            var category = await _context.Categories.FindAsync(product.CategoryId);
            if (category == null)
                return BadRequest(new { error = "Invalid CategoryId" });

            _context.Entry(product).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Products.Any(e => e.Id == id))
                    return NotFound(new { error = "Product not found" });
                else
                    throw;
            }

            return NoContent();
        }

        // DELETE: /Product/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound(new { error = "Product not found" });

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
