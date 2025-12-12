namespace backend.Models;

public enum AppointmentStatus
{
    Scheduled,
    Completed,
    Cancelled,
    Rescheduled
}

public class Appointment
{
    public int Id { get; set; }
    public int BusinessId { get; set; }
    public int? ServiceId { get; set; }
    public int? EmployeeId { get; set; }
    public DateTime Date { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;
    public int? OrderId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public Business Business { get; set; } = null!;
    public Service? Service { get; set; }
    public User? Employee { get; set; }
    public Order? Order { get; set; }
}
