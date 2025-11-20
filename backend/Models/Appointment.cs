namespace backend.Models;

public class Appointment
{
    public int Id { get; set; }
    
    public int EmployeeId { get; set; }
    public int ServiceId { get; set; }
    
    public DateTime BookedAt { get; set; } // better datetime than string like in pdf
    public DateTime AppointmentTime { get; set; } // better datetime than string like in pdf
    public int DurationMinutes { get; set; }
    
    public string ServiceTitle { get; set; } = default!; // could use enum
    public string ServiceProviderName { get; set; } = default!;
    public string CustomerName { get; set; } = default!;
    public string CustomerEmail { get; set; } = default!;
    public string CustomerPhone { get; set; } = default!;
    
    public string Status { get; set; } = default!; // e.g. "scheduled", "completed", "canceled"
    
    // Navigation
    public Employee Employee { get; set; } = default!;
    public Service Service { get; set; } = default!;
}