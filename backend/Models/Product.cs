namespace backend.Models;

public class Product
{
    public Product()
    {
        CreatedAt = DateTime.UtcNow;
    }

    public int Id { get; set; }
    public int BusinessId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public List<string> Tags { get; set; } = new List<string>();
    public bool Available { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public Business Business { get; set; } = null!;
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public ICollection<ProductModificationAssignment> ModificationAssignments { get; set; } = new List<ProductModificationAssignment>();
    public ICollection<InventoryItem> InventoryItems { get; set; } = new List<InventoryItem>();
}
