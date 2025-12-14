using System.ComponentModel.DataAnnotations;

namespace backend.DTOs;

public class CreateEmployeeRequest
{
    [Required(ErrorMessage = "Name is required")]
    [MaxLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
    public string Name { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Password is required")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
    public string Password { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Phone is required")]
    [MaxLength(50, ErrorMessage = "Phone cannot exceed 50 characters")]
    public string Phone { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Role is required")]
    public string Role { get; set; } = "Employee"; // Employee, Manager, Admin
}
