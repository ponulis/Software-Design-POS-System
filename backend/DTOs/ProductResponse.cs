namespace backend.DTOs;

public class ProductResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public List<string> Tags { get; set; } = new();
    public bool Available { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Modification attributes assigned to this product
    public List<ProductModificationResponse> Modifications { get; set; } = new();
    
    // Inventory items for this product
    public List<InventoryItemResponse> InventoryItems { get; set; } = new();
}
