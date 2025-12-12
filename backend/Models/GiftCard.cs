namespace backend.Models;

public class GiftCard
{
    public int Id { get; set; }
    public int BusinessId { get; set; }
    public string Code { get; set; } = string.Empty; // Unique identifier
    public decimal Balance { get; set; }
    public decimal OriginalAmount { get; set; }
    public DateTime IssuedDate { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiryDate { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public Business Business { get; set; } = null!;
}
