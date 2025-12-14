namespace backend.DTOs;

public class UpdateDiscountRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Type { get; set; } // Percentage or FixedAmount
    public decimal? Value { get; set; }
    public bool? IsActive { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
}
