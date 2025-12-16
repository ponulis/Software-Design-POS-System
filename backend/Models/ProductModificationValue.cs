namespace backend.Models;

public class ProductModificationValue
{
    public ProductModificationValue()
    {
        CreatedAt = DateTime.UtcNow;
    }

    public int Id { get; set; }
    public int ModificationId { get; set; }
    public string Value { get; set; } = string.Empty; // e.g., "Red", "Blue", "Large", "Small"
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public ProductModification Modification { get; set; } = null!;
    public ICollection<InventoryModificationValue> InventoryItems { get; set; } = new List<InventoryModificationValue>();
}
