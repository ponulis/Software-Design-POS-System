namespace backend.DTOs;

public class CreateStripePaymentIntentRequest
{
    public int OrderId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "usd";
    public string? PaymentMethodId { get; set; } // Stripe PaymentMethod ID from frontend
}
