namespace backend.Models;

public class Discount
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string Description { get; set; }
    public decimal Amount { get; set; }
    public string AmountType { get; set; } = default!; // e.g., "Percentage" or "Fixed"
    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; }
    
    // Navigation
    public Business Business { get; set; } = default!;
    public List<Order> Orders { get; set; } = new();


}