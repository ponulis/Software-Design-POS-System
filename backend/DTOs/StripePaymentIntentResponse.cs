namespace backend.DTOs;

public class StripePaymentIntentResponse
{
    public string ClientSecret { get; set; } = string.Empty;
    public string PaymentIntentId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
