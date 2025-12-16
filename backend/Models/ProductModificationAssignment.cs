namespace backend.Models;

/// <summary>
/// Represents which modifications (attributes) are assigned to a product
/// A product can have multiple modifications (e.g., Color and Size)
/// </summary>
public class ProductModificationAssignment
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int ModificationId { get; set; }

    // Navigation properties
    public Product Product { get; set; } = null!;
    public ProductModification Modification { get; set; } = null!;
}
