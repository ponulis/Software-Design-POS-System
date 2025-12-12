namespace backend.DTOs;

public class CreateOrderRequest
{
    public int SpotId { get; set; }
    public int CreatedBy { get; set; }
    public List<CreateOrderItemRequest> Items { get; set; } = new();
    public decimal? SubTotal { get; set; }
    public decimal? Discount { get; set; }
    public decimal? Tax { get; set; }
    public string? Status { get; set; }
}
