using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace OrderService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly OrderDbContext _context;

        public OrderController(OrderDbContext context)
        {
            _context = context;
        }

        // GET: /Order
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> Get()
        {
            return Ok(await _context.Orders.ToListAsync());
        }

        // GET: /Order/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Order>> Get(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
                return NotFound(new { error = "Order not found" });
            return Ok(order);
        }

        // POST: /Order
        [HttpPost]
        public async Task<ActionResult<Order>> Post([FromBody] Order order)
        {
            if (order.ProductId <= 0)
                return BadRequest(new { error = "ProductId is required" });
            if (order.Quantity <= 0)
                return BadRequest(new { error = "Quantity must be greater than zero" });

            order.OrderDate = DateTime.UtcNow; // set order date

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = order.Id }, order);
        }

        // PUT: /Order/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Order order)
        {
            if (id != order.Id)
                return BadRequest(new { error = "ID mismatch" });

            if (order.ProductId <= 0)
                return BadRequest(new { error = "ProductId is required" });
            if (order.Quantity <= 0)
                return BadRequest(new { error = "Quantity must be greater than zero" });

            _context.Entry(order).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Orders.Any(e => e.Id == id))
                    return NotFound(new { error = "Order not found" });
                else
                    throw;
            }

            return NoContent();
        }

        // DELETE: /Order/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
                return NotFound(new { error = "Order not found" });

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
