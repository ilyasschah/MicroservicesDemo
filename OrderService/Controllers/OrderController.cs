using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OrderService;


[ApiController]
[Route("[controller]")]
public class OrderController : ControllerBase
{
    private readonly OrderDbContext _context;

    public OrderController(OrderDbContext context)
    {
        _context = context;
    }
    public class InventoryCheckDecrementDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
    // GET: /Order
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Order>>> Get()
    {
        var orders = await _context.Orders
            .Include(o => o.Items)
            .ToListAsync();
        return Ok(orders);
    }

    // GET: /Order/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<Order>> Get(int id)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
            return NotFound(new { error = "Order not found" });

        return Ok(order);
    }

    // POST: /Order
    [HttpPost]
    public async Task<ActionResult<Order>> Post([FromBody] Order order)
    {
        if (order.Items == null || !order.Items.Any())
            return BadRequest(new { error = "Order must have at least one item." });

        // Prepare inventory check list
        var inventoryUpdates = order.Items.Select(i => new InventoryCheckDecrementDto
        {
            ProductId = i.ProductId,
            Quantity = i.Quantity
        }).ToList();

        // Call ProductService Inventory API
        using (var httpClient = new HttpClient())
        {
            // If running behind YARP or ApiGateway, use that URL, e.g. http://localhost:5034/api/Inventory/check-and-decrement
            var productServiceUrl = "http://localhost:5034/api/Inventory/check-and-decrement";
            var response = await httpClient.PostAsJsonAsync(productServiceUrl, inventoryUpdates);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return BadRequest(new { error = $"Inventory error: {error}" });
            }
        }

        order.Date = DateTime.UtcNow;
        order.Total = order.Items.Sum(i => i.Price * i.Quantity);

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

        // Optional: Validate items, just like in Post

        // Attach existing items and order for update
        var existingOrder = await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (existingOrder == null)
            return NotFound(new { error = "Order not found" });

        // Update fields
        existingOrder.Date = order.Date;
        existingOrder.CustomerId = order.CustomerId;
        existingOrder.Status = order.Status;
        existingOrder.Total = order.Items.Sum(i => i.Price * i.Quantity);

        // Update items
        _context.OrderItems.RemoveRange(existingOrder.Items);
        existingOrder.Items = order.Items;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    // DELETE: /Order/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
            return NotFound(new { error = "Order not found" });

        _context.Orders.Remove(order);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
