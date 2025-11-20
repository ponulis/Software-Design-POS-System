namespace backend.Models;

public class OrderItem
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int OrderId { get; set; }
    
    public int VariationId { get; set; }
    public decimal DiscountAmount { get; set; } = default!;
    public int Quantity { get; set; }

    public Order Order { get; set; } = default!;
    public List<Product> Products { get; set; }
}