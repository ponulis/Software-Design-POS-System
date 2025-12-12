namespace backend.Models;

public class Tax
{
    public int Id { get; set; }
    public int BusinessId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Rate { get; set; } // Percentage (e.g., 21.0 for 21%)
    public bool IsActive { get; set; } = true;
    public DateTime EffectiveFrom { get; set; } = DateTime.UtcNow;
    public DateTime? EffectiveTo { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Business Business { get; set; } = null!;
}
