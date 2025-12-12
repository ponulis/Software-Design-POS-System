namespace backend.DTOs;

public class CreatePaymentRequest
{
    public int OrderId { get; set; }
    public decimal Amount { get; set; }
    public string Method { get; set; } = string.Empty; // Cash, Card, GiftCard
    public string? TransactionId { get; set; }
    public string? AuthorizationCode { get; set; }
}
