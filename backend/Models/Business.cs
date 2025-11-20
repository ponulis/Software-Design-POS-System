namespace backend.Models;

public class Business
{
    public int Id { get; set; }
    public int OwnerId { get; set; }
    public string Name { get; set; } = default!;
    public string ? Description { get; set; }
    public string? Address { get; set; }
    public string? PhoneNumber { get; set; }
    public string ? Email { get; set; }

    public List<Employee> Employees { get; set; } = new();
    public List<Tax> Taxes { get; set; } = new();
    public List<Product> Products { get; set; } = new();
    public List<Service> Services { get; set; } = new();
    public List<Discount> Discounts { get; set; } = new();
}