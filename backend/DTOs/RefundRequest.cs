using System.ComponentModel.DataAnnotations;

namespace backend.DTOs;

public class RefundRequest
{
    [MaxLength(500, ErrorMessage = "Reason cannot exceed 500 characters")]
    public string? Reason { get; set; }
    
    public decimal? Amount { get; set; } // Optional: partial refund amount. If null, refund full order total
}
