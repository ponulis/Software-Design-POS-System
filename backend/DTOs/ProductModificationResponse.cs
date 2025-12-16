namespace backend.DTOs;

public class ProductModificationResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<ProductModificationValueResponse> Values { get; set; } = new();
    
    // Pricing options
    public string PriceType { get; set; } = "None"; // "None", "Fixed", "Percentage"
    public decimal? FixedPriceAddition { get; set; }
    public decimal? PercentagePriceIncrease { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class ProductModificationValueResponse
{
    public int Id { get; set; }
    public string Value { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
