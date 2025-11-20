namespace backend.Models;

public class Payment
{
    public int Id { get; set; }
    public int OrderId { get; set; }

    public decimal Amount { get; set; }
    public string Method { get; set; } = default!; 
    // "card", "cash", "giftCard", "split"

    public string? ExternalTransactionId { get; set; } // Stripe
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Order Order { get; set; } = default!;
    public List<GiftCard> GiftCards { get; set; } = new();
}