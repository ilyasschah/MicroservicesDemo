using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace ProductService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryController : ControllerBase
    {
        private readonly ProductDbContext _context;

        public InventoryController(ProductDbContext context)
        {
            _context = context;
        }

        // GET: api/Inventory
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Inventory>>> GetAll()
        {
            var list = await _context.Inventories.ToListAsync();
            return Ok(list);
        }

        // GET: api/Inventory/{productId}
        [HttpGet("{productId}")]
        public async Task<ActionResult<Inventory>> GetByProduct(int productId)
        {
            var inventory = await _context.Inventories
                                          .FirstOrDefaultAsync(i => i.ProductId == productId);
            if (inventory == null)
                return NotFound(new { error = "Inventory not found for product " + productId });
            return Ok(inventory);
        }

        // POST: api/Inventory
        [HttpPost]
        public async Task<ActionResult<Inventory>> Create([FromBody] Inventory inventory)
        {
            // Validate product exists
            var product = await _context.Products.FindAsync(inventory.ProductId);
            if (product == null)
                return BadRequest(new { error = "Product does not exist" });

            // Prevent duplicate inventory entries
            var existing = await _context.Inventories
                                         .FirstOrDefaultAsync(i => i.ProductId == inventory.ProductId);
            if (existing != null)
            {
                existing.Stock = inventory.Stock;
                await _context.SaveChangesAsync();
                return Ok(existing);
            }

            _context.Inventories.Add(inventory);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetByProduct),
                                   new { productId = inventory.ProductId },
                                   inventory);
        }
        // Add at the end of InventoryController class
        [HttpPost("check-and-decrement")]
        public async Task<IActionResult> CheckAndDecrement([FromBody] List<InventoryCheckDecrementDto> updates)
        {
            foreach (var update in updates)
            {
                var inventory = await _context.Inventories
                    .FirstOrDefaultAsync(i => i.ProductId == update.ProductId);

                if (inventory == null)
                    return NotFound(new { error = $"ProductId {update.ProductId} not found in inventory." });

                if (inventory.Stock < update.Quantity)
                    return BadRequest(new { error = $"Not enough stock for ProductId {update.ProductId}." });

                inventory.Stock -= update.Quantity;
                _context.Entry(inventory).State = EntityState.Modified;
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        public class InventoryCheckDecrementDto
        {
            public int ProductId { get; set; }
            public int Quantity { get; set; }
        }
        // PUT: api/Inventory/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStock(int id, [FromBody] InventoryUpdateDto dto)
        {
            if (id != dto.Id)
                return BadRequest(new { error = "ID in URL and body must match." });

            var inventory = await _context.Inventories.FindAsync(id);
            if (inventory == null)
                return NotFound(new { error = "Inventory entry not found for ID " + id });

            if (dto.Stock < 0)
                return BadRequest(new { error = "Stock cannot be negative." });

            inventory.Stock = dto.Stock;
            _context.Inventories.Update(inventory);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Inventory/{productId}
        [HttpDelete("{productId}")]
        public async Task<IActionResult> DeleteByProduct(int productId)
        {
            var inventory = await _context.Inventories
                                          .FirstOrDefaultAsync(i => i.ProductId == productId);
            if (inventory == null)
                return NotFound(new { error = "Inventory not found for product " + productId });

            _context.Inventories.Remove(inventory);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }

    // DTO for stock update
    public class InventoryUpdateDto
    {
        public int Id { get; set; }
        public int Stock { get; set; }
    }
}
