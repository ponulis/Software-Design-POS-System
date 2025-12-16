namespace backend.DTOs;

public class CreateProductModificationRequest
{
    public string Name { get; set; } = string.Empty;
    public List<string> Values { get; set; } = new(); // Initial values for this modification
    
    // Pricing options
    public string PriceType { get; set; } = "None"; // "None", "Fixed", "Percentage"
    public decimal? FixedPriceAddition { get; set; } // Fixed amount to add (e.g., 5.00)
    public decimal? PercentagePriceIncrease { get; set; } // Percentage increase (e.g., 10.00 for 10%)
}
