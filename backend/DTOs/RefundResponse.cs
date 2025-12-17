namespace backend.DTOs;

public class RefundResponse
{
    public int OrderId { get; set; }
    public decimal RefundAmount { get; set; }
    public string RefundMethod { get; set; } = string.Empty; // "Full" or "Partial"
    public string? Reason { get; set; }
    public List<RefundPaymentResponse> RefundedPayments { get; set; } = new();
    public string OrderStatus { get; set; } = string.Empty;
    public DateTime RefundedAt { get; set; }
}

public class RefundPaymentResponse
{
    public int PaymentId { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public decimal RefundedAmount { get; set; }
    public string? RefundTransactionId { get; set; } // Refund transaction ID (if applicable)
    public string Status { get; set; } = string.Empty; // "Success", "Failed", "Pending"
}
