namespace backend.DTOs;

public class UpdateServiceRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public int? DurationMinutes { get; set; }
    public bool? Available { get; set; }
}
