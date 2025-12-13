namespace backend.DTOs;

public class CreateGiftCardRequest
{
    public string Code { get; set; } = string.Empty; // Unique code (if not provided, will be generated)
    public decimal OriginalAmount { get; set; }
    public DateTime? ExpiryDate { get; set; }
}
