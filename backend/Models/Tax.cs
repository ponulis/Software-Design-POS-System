namespace backend.Models;

public class Tax
{
    public int Id { get; set; }
    public int BusinessId { get; set; }
    public string Name { get; set; } = default!;
    public decimal Rate { get; set; }
    public string AppliesTo { get; set; } = default!;
    public DateTime EffectiveFrom { get; set; }
    public DateTime EffectiveTo { get; set; }

    public Business Business { get; set; } = default!;
    public List<Service> Services { get; set; } = default!;
    public List<Product> Products { get; set; } = default!;
}