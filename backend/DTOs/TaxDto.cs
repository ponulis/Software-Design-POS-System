namespace backend.DTOs;

public class TaxCreateDto
{
    public int BusinessId { get; set; }
    public string Name { get; set; } = "";
    public decimal Rate { get; set; }
    public string AppliesTo { get; set; } = "";
    public DateTime EffectiveFrom { get; set; }
    public DateTime EffectiveTo { get; set; }
}

public class TaxUpdateDto
{
    public string? Name { get; set; }
    public decimal? Rate { get; set; }
    public string? AppliesTo { get; set; }
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
}

public class TaxReadDto
{
    public int Id { get; set; }
    public int BusinessId { get; set; }
    public string Name { get; set; } = "";
    public decimal Rate { get; set; }
    public string AppliesTo { get; set; } = "";
    public DateTime EffectiveFrom { get; set; }
    public DateTime EffectiveTo { get; set; }
}
