namespace backend.DTOs;

public class CreateAppointmentRequest
{
    public int? ServiceId { get; set; }
    public int? EmployeeId { get; set; }
    public DateTime Date { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public int? OrderId { get; set; }
}
