namespace ProductService
{
    public class Inventory
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int Stock { get; set; }
        public Product? Product { get; set; }
    }
}
