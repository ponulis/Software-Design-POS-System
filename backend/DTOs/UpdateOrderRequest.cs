namespace backend.DTOs;

public class UpdateOrderRequest
{
    public int? SpotId { get; set; }
    public List<CreateOrderItemRequest>? Items { get; set; }
    public decimal? SubTotal { get; set; }
    public decimal? Discount { get; set; }
    public decimal? Tax { get; set; }
    public string? Status { get; set; }
}
