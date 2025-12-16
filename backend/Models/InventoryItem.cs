namespace backend.Models;

/// <summary>
/// Represents inventory stock for a product with specific modification values
/// For example: Product "T-Shirt" with Color="Red" and Size="Large" has Quantity=10
/// </summary>
public class InventoryItem
{
    public InventoryItem()
    {
        CreatedAt = DateTime.UtcNow;
    }

    public int Id { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; } = 0;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public Product Product { get; set; } = null!;
    
    // Store modification values as JSON or separate table
    // Using a JSON string to store the combination of modification values
    // Format: {"Color": "Red", "Size": "Large"}
    public string ModificationValuesJson { get; set; } = "{}";
    
    // For easier querying, we'll also store individual modification value IDs
    // This allows us to query by specific modification values
    public ICollection<InventoryModificationValue> ModificationValues { get; set; } = new List<InventoryModificationValue>();
}

/// <summary>
/// Junction table linking InventoryItem to ProductModificationValue
/// Allows querying inventory by specific modification values
/// </summary>
public class InventoryModificationValue
{
    public int Id { get; set; }
    public int InventoryItemId { get; set; }
    public int ModificationValueId { get; set; }

    // Navigation properties
    public InventoryItem InventoryItem { get; set; } = null!;
    public ProductModificationValue ModificationValue { get; set; } = null!;
}
