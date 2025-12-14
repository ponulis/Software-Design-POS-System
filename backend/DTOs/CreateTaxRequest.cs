using System.ComponentModel.DataAnnotations;

namespace backend.DTOs;

public class CreateTaxRequest
{
    [Required(ErrorMessage = "Name is required")]
    [MaxLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
    public string Name { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Rate is required")]
    [Range(0, 100, ErrorMessage = "Rate must be between 0 and 100")]
    public decimal Rate { get; set; } // Percentage (e.g., 21.0 for 21%)
    
    public bool IsActive { get; set; } = true;
    
    public DateTime? EffectiveFrom { get; set; }
    
    public DateTime? EffectiveTo { get; set; }
}
