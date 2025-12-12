namespace backend.DTOs;

public class CreateOrderItemRequest
{
    public int? OrderId { get; set; } // Optional - not needed when creating order items as part of order creation
    public int MenuId { get; set; } // Maps to Product.Id
    public int Quantity { get; set; }
    public decimal? Price { get; set; } // Optional, will use Product.Price if not provided
    public string? Notes { get; set; }
}
