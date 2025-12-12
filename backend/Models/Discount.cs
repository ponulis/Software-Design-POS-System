namespace backend.Models;

public enum DiscountType
{
    Percentage,
    FixedAmount
}

public class Discount
{
    public int Id { get; set; }
    public int BusinessId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DiscountType Type { get; set; }
    public decimal Value { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Business Business { get; set; } = null!;
}
