namespace backend.DTOs;

public class UpdateOrderItemRequest
{
    public int? MenuId { get; set; }
    public int? Quantity { get; set; }
    public decimal? Price { get; set; }
    public string? Notes { get; set; }
}
