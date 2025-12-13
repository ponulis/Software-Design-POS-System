namespace backend.DTOs;

public class CreateSplitPaymentRequest
{
    public int OrderId { get; set; }
    public List<SplitPaymentItem> Payments { get; set; } = new();
}

public class SplitPaymentItem
{
    public decimal Amount { get; set; }
    public string Method { get; set; } = string.Empty; // Cash, Card, GiftCard
    public List<int>? OrderItemIds { get; set; } // Optional: specific order items this payment covers
    public decimal? CashReceived { get; set; } // Required for cash payments
    public string? GiftCardCode { get; set; } // Required for gift card payments
    public string? PaymentIntentId { get; set; } // Required for card payments (Stripe PaymentIntent ID)
}
