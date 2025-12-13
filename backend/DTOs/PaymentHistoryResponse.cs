namespace backend.DTOs;

public class PaymentHistoryResponse
{
    public int PaymentId { get; set; }
    public int OrderId { get; set; }
    public decimal Amount { get; set; }
    public string Method { get; set; } = string.Empty;
    public DateTime PaidAt { get; set; }
    public int CreatedBy { get; set; }
    public string? CreatedByName { get; set; }
    public string? TransactionId { get; set; }
    public string? AuthorizationCode { get; set; }
    public decimal? CashReceived { get; set; }
    public decimal? Change { get; set; }
}
