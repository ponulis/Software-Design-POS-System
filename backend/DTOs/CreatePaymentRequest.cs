namespace backend.DTOs;

public class CreatePaymentRequest
{
    public int OrderId { get; set; }
    public decimal Amount { get; set; }
    public string Method { get; set; } = string.Empty; // Cash, Card, GiftCard
    public decimal? CashReceived { get; set; } // Required for cash payments - amount of cash received from customer
    public string? GiftCardCode { get; set; } // Required for gift card payments - gift card code
    public string? TransactionId { get; set; }
    public string? AuthorizationCode { get; set; }
    
    // Card payment details (for mocked card payments)
    public CardDetails? CardDetails { get; set; }
}

public class CardDetails
{
    public string CardNumber { get; set; } = string.Empty;
    public string ExpiryMonth { get; set; } = string.Empty;
    public string ExpiryYear { get; set; } = string.Empty;
    public string Cvv { get; set; } = string.Empty;
    public string CardholderName { get; set; } = string.Empty;
}
