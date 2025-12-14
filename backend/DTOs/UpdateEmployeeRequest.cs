namespace backend.DTOs;

public class UpdateEmployeeRequest
{
    public string? Name { get; set; }
    public string? Password { get; set; }
    public string? Phone { get; set; }
    public string? Role { get; set; } // Employee, Manager, Admin
    public bool? IsActive { get; set; }
}
