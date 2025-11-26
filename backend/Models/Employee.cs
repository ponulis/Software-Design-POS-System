namespace backend.Models;

public class Employee
{
    public int Id { get; set; }
    public int BusinessId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "User"; // e.g. "cashier", "admin" maybe we don't need it

    
    public Business Business { get; set; } = default!;
    public List<Appointment> Appointments { get; set; } = new();
    public List<Order> Orders { get; set; } = new();
}