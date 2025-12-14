using System.ComponentModel.DataAnnotations;

namespace backend.DTOs;

public class CreateDiscountRequest
{
    [Required(ErrorMessage = "Name is required")]
    [MaxLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
    public string Name { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    [Required(ErrorMessage = "Type is required")]
    public string Type { get; set; } = string.Empty; // Percentage or FixedAmount
    
    [Required(ErrorMessage = "Value is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Value must be greater than 0")]
    public decimal Value { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime? ValidFrom { get; set; }
    
    public DateTime? ValidTo { get; set; }
}
