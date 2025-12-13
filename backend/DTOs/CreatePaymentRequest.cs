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
}
