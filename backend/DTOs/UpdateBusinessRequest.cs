using System.ComponentModel.DataAnnotations;

namespace backend.DTOs;

public class UpdateBusinessRequest
{
    [MaxLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
    public string? Name { get; set; }
    
    [MaxLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string? Description { get; set; }
    
    [MaxLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
    public string? Address { get; set; }
    
    [EmailAddress(ErrorMessage = "Invalid email address")]
    [MaxLength(200, ErrorMessage = "Contact email cannot exceed 200 characters")]
    public string? ContactEmail { get; set; }
    
    [MaxLength(50, ErrorMessage = "Phone number cannot exceed 50 characters")]
    public string? PhoneNumber { get; set; }
}
