namespace backend.DTOs;

public class CancelAppointmentRequest
{
    public string? CancellationReason { get; set; }
    public string? Notes { get; set; }
}
