namespace backend.DTOs;

public class UpdateAppointmentRequest
{
    public int? ServiceId { get; set; }
    public int? EmployeeId { get; set; }
    public DateTime? Date { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public string? Notes { get; set; }
    public string? Status { get; set; } // Scheduled, Completed, Cancelled, Rescheduled
    public int? OrderId { get; set; }
}
