using backend.DTOs;

namespace backend.DTOs;

public class PaymentConfirmationResponse
{
    public PaymentResponse Payment { get; set; } = null!;
    public OrderResponse Order { get; set; } = null!;
    public string Message { get; set; } = string.Empty;
}
