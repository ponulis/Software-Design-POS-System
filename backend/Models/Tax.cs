namespace backend.Models;

public class Tax
{
    public Tax()
    {
        EffectiveFrom = DateTime.UtcNow;
        CreatedAt = DateTime.UtcNow;
    }

    public int Id { get; set; }
    public int BusinessId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Rate { get; set; } // Percentage (e.g., 21.0 for 21%)
    public bool IsActive { get; set; } = true;
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public Business Business { get; set; } = null!;
}
