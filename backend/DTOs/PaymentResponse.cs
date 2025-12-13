namespace backend.DTOs;

public class PaymentResponse
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int CreatedBy { get; set; }
    public decimal Amount { get; set; }
    public string Method { get; set; } = string.Empty;
    public decimal? CashReceived { get; set; } // Amount of cash received (for cash payments)
    public decimal? Change { get; set; } // Change amount (cashReceived - amount, for cash payments)
    public DateTime PaidAt { get; set; }
    public string? TransactionId { get; set; }
    public string? AuthorizationCode { get; set; }
}
