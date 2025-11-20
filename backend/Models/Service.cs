namespace backend.Models;

public class Service
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public decimal Price { get; set; }
    public decimal TaxRate { get; set; }
    public int BusinessId { get; set; }
    public int EmployeeId { get; set; }
    public decimal DurationMinutes { get; set; }
    public int TaxId { get; set; }
    
    // Navigation
    public Business Business { get; set; } = default!;
    public List<Appointment> Appointments { get; set; } = default!;
    public Tax Tax { get; set; } = default!;
}