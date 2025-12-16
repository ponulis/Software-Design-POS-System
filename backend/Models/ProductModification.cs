namespace backend.Models;

public enum PriceModificationType
{
    None = 0,
    Fixed = 1,
    Percentage = 2
}

public class ProductModification
{
    public ProductModification()
    {
        CreatedAt = DateTime.UtcNow;
        Values = new List<ProductModificationValue>();
    }

    public int Id { get; set; }
    public int BusinessId { get; set; }
    public string Name { get; set; } = string.Empty; // e.g., "Color", "Size"
    
    // Pricing options
    public PriceModificationType PriceType { get; set; } = PriceModificationType.None;
    public decimal? FixedPriceAddition { get; set; } // Fixed amount to add (e.g., 5.00)
    public decimal? PercentagePriceIncrease { get; set; } // Percentage increase (e.g., 10.00 for 10%)
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public Business Business { get; set; } = null!;
    public ICollection<ProductModificationValue> Values { get; set; }
    public ICollection<ProductModificationAssignment> ProductAssignments { get; set; } = new List<ProductModificationAssignment>();
}
