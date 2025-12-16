namespace backend.DTOs;

public class InventoryItemResponse
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public Dictionary<string, string> ModificationValues { get; set; } = new(); // e.g., {"Color": "Red", "Size": "Large"}
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
