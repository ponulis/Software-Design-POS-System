namespace backend.DTOs;

public class AppointmentResponse
{
    public int Id { get; set; }
    public int BusinessId { get; set; }
    public int? ServiceId { get; set; }
    public string? ServiceName { get; set; }
    public int? EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public DateTime Date { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string Status { get; set; } = string.Empty;
    public int? OrderId { get; set; }
    public string? OrderStatus { get; set; } // Payment status via Order.Status
    public decimal? OrderTotal { get; set; } // Order total if OrderId exists
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
