namespace backend.Models;

public enum PaymentMethod
{
    Cash,
    Card,
    GiftCard
}

public class Payment
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int CreatedBy { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod Method { get; set; }
    public DateTime PaidAt { get; set; } = DateTime.UtcNow;
    public string? TransactionId { get; set; }
    public string? AuthorizationCode { get; set; }

    // Navigation properties
    public Order Order { get; set; } = null!;
    public User Creator { get; set; } = null!;
}
