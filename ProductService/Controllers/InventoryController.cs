using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProductService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class InventoryController : ControllerBase
    {
        private readonly ProductDbContext _context;

        public InventoryController(ProductDbContext context)
        {
            _context = context;
        }

        // GET: /Inventory
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Inventory>>> Get()
        {
            return Ok(await _context.Inventories.ToListAsync());
        }

        // GET: /Inventory/{productId}
        [HttpGet("{productId}")]
        public async Task<ActionResult<Inventory>> Get(int productId)
        {
            var inventory = await _context.Inventories.FirstOrDefaultAsync(i => i.ProductId == productId);
            if (inventory == null)
                return NotFound(new { error = "Inventory not found for this product" });
            return Ok(inventory);
        }

        // POST: /Inventory
        [HttpPost]
        public async Task<ActionResult<Inventory>> Post([FromBody] Inventory inventory)
        {
            // Ensure Product exists
            var product = await _context.Products.FindAsync(inventory.ProductId);
            if (product == null)
                return BadRequest(new { error = "Product does not exist" });

            // Add or update
            var existing = await _context.Inventories.FirstOrDefaultAsync(i => i.ProductId == inventory.ProductId);
            if (existing != null)
            {
                existing.Stock = inventory.Stock;
                await _context.SaveChangesAsync();
                return Ok(existing);
            }

            _context.Inventories.Add(inventory);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { productId = inventory.ProductId }, inventory);
        }

        // PUT: /Inventory/{productId}
        [HttpPut("{productId}")]
        public async Task<IActionResult> UpdateStock(int productId, [FromBody] int newStock)
        {
            var inventory = await _context.Inventories.FirstOrDefaultAsync(i => i.ProductId == productId);
            if (inventory == null)
            {
                return NotFound(new { error = "Inventory not found for product ID " + productId });
            }

            if (newStock < 0)
            {
                return BadRequest(new { error = "Stock cannot be negative" });
            }

            inventory.Stock = newStock;
            await _context.SaveChangesAsync();

            return Ok(inventory);
        }

        // DELETE: /Inventory/{productId}
        [HttpDelete("{productId}")]
        public async Task<IActionResult> Delete(int productId)
        {
            var inventory = await _context.Inventories.FirstOrDefaultAsync(i => i.ProductId == productId);
            if (inventory == null)
                return NotFound(new { error = "Inventory not found" });

            _context.Inventories.Remove(inventory);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
