namespace backend.DTOs;

public class DiscountCreateDto
{
    public int BusinessId { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public decimal Amount { get; set; }
    public string AmountType { get; set; } = ""; // "Fixed" or "Percentage"
    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; }
}

public class DiscountUpdateDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal? Amount { get; set; }
    public string? AmountType { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
}

public class DiscountReadDto
{
    public int Id { get; set; }
    public int BusinessId { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public decimal Amount { get; set; }
    public string AmountType { get; set; } = "";
    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; }
}
