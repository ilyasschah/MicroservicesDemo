using System;
using System.Collections.Generic;

public class Order
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public int CustomerId { get; set; }
    public decimal Total { get; set; }
    public string? Status { get; set; }
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}
