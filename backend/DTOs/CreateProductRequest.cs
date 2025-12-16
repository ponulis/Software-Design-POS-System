namespace backend.DTOs;

public class CreateProductRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public List<string> Tags { get; set; } = new();
    public bool Available { get; set; } = true;
    
    // Modification attributes (e.g., ["Color", "Size"])
    public List<int> ModificationIds { get; set; } = new();
    
    // Inventory items with modification value combinations
    public List<CreateInventoryItemRequest> InventoryItems { get; set; } = new();
}

public class CreateInventoryItemRequest
{
    // Dictionary mapping modification name to modification value ID
    // e.g., {"Color": 1, "Size": 3} means Color=Red (ID 1), Size=Large (ID 3)
    public Dictionary<string, int> ModificationValueIds { get; set; } = new();
    public int Quantity { get; set; }
}
