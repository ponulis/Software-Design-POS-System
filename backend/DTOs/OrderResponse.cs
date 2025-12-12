using backend.Models;

namespace backend.DTOs;

public class OrderResponse
{
    public int Id { get; set; }
    public int SpotId { get; set; }
    public int CreatedBy { get; set; }
    public List<OrderItemResponse> Items { get; set; } = new();
    public decimal SubTotal { get; set; }
    public decimal Discount { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class OrderItemResponse
{
    public int Id { get; set; }
    public int MenuId { get; set; } // Maps to Product.Id
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public string? Notes { get; set; }
}
