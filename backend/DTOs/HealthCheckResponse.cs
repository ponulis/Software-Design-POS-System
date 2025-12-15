namespace backend.DTOs;

/// <summary>
/// Health check response DTO
/// </summary>
public class HealthCheckResponse
{
    /// <summary>
    /// Overall health status
    /// </summary>
    public string Status { get; set; } = "Healthy";

    /// <summary>
    /// Timestamp of the health check
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Application version or build information
    /// </summary>
    public string? Version { get; set; }

    /// <summary>
    /// Database connectivity status
    /// </summary>
    public string DatabaseStatus { get; set; } = "Unknown";

    /// <summary>
    /// Additional details about the health check
    /// </summary>
    public Dictionary<string, object>? Details { get; set; }
}
