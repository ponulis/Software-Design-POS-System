namespace backend.DTOs;

public class ProductCreateDto
{
    public int BusinessId { get; set; }
    public int TaxId { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}

public class ProductUpdateDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public int? TaxId { get; set; }
    public int Quantity { get; set; }
}

public class ProductReadDto
{
    public int Id { get; set; }
    public int BusinessId { get; set; }
    public int TaxId { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public decimal Price { get; set; }
    public decimal TaxRate { get; set; }
    public int Quantity{ get; set; }
}
