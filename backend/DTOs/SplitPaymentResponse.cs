using backend.DTOs;

namespace backend.DTOs;

public class SplitPaymentResponse
{
    public int OrderId { get; set; }
    public List<PaymentResponse> Payments { get; set; } = new();
    public decimal TotalPaid { get; set; }
    public decimal RemainingBalance { get; set; }
    public bool IsFullyPaid { get; set; }
    public OrderResponse Order { get; set; } = null!;
}
