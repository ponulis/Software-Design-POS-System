using System.ComponentModel.DataAnnotations;

namespace backend.DTOs;

public class CreateAppointmentRequest
{
    public int? ServiceId { get; set; }
    public int? EmployeeId { get; set; }
    
    [Required(ErrorMessage = "Date is required")]
    public DateTime Date { get; set; }
    
    [Required(ErrorMessage = "Customer name is required")]
    [MaxLength(200, ErrorMessage = "Customer name cannot exceed 200 characters")]
    public string CustomerName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Customer phone is required")]
    [MaxLength(50, ErrorMessage = "Customer phone cannot exceed 50 characters")]
    public string CustomerPhone { get; set; } = string.Empty;
    
    public string? Notes { get; set; }
    public int? OrderId { get; set; }
}
