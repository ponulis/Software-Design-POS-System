namespace backend.Models;

public class Employee
{
    public int Id { get; set; }
    public int BusinessId { get; set; }
    public string Name { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;
    public string Role { get; set; } = default!; // e.g. "cashier", "admin"

    // Navigation
    public Business Business { get; set; } = default!;
    public List<Appointment> Appointments { get; set; } = new();
    public List<Order> Orders { get; set; } = new();
}