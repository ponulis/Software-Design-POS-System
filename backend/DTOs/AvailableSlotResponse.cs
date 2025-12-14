namespace backend.DTOs;

public class AvailableSlotResponse
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int? EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public int? ServiceId { get; set; }
    public string? ServiceName { get; set; }
}
