namespace backend.DTOs;

public class ConfirmStripePaymentRequest
{
    public int OrderId { get; set; }
    public string PaymentIntentId { get; set; } = string.Empty;
}
