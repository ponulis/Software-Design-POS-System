namespace backend.DTOs;

public class UpdateProductRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public List<string>? Tags { get; set; }
    public bool? Available { get; set; }
}
