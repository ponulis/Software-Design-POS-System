namespace backend.DTOs;

public class UpdateTaxRequest
{
    public string? Name { get; set; }
    public decimal? Rate { get; set; }
    public bool? IsActive { get; set; }
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
}
