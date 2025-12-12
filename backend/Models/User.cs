namespace backend.Models;

public class User
{
    public int Id { get; set; }
    public int BusinessId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Role { get; set; } = "Employee"; // Employee, Manager, Admin
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public Business Business { get; set; } = null!;
    public ICollection<Order> OrdersCreated { get; set; } = new List<Order>();
    public ICollection<Payment> PaymentsCreated { get; set; } = new List<Payment>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
