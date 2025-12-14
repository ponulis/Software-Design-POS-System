namespace backend.DTOs;

/// <summary>
/// Error response schema per API contract
/// </summary>
public class ErrorResponse
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
