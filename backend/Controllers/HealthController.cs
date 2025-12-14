using backend.Data;
using backend.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

/// <summary>
/// Health check endpoint for monitoring and deployment verification
/// </summary>
[ApiController]
[Route("api/health")]
public class HealthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<HealthController> _logger;

    public HealthController(ApplicationDbContext context, ILogger<HealthController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Health check endpoint
    /// </summary>
    /// <returns>Health status of the application</returns>
    /// <remarks>
    /// This endpoint can be used by monitoring systems, load balancers, and deployment tools
    /// to verify that the API is running and the database is accessible.
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(HealthCheckResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(HealthCheckResponse), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetHealth()
    {
        var response = new HealthCheckResponse
        {
            Timestamp = DateTime.UtcNow,
            Version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString(),
            Details = new Dictionary<string, object>()
        };

        var isHealthy = true;

        // Check database connectivity
        try
        {
            var canConnect = await _context.Database.CanConnectAsync();
            if (canConnect)
            {
                // Try a simple query to ensure database is responsive
                await _context.Database.ExecuteSqlRawAsync("SELECT 1");
                response.DatabaseStatus = "Connected";
                response.Details["Database"] = "Available";
            }
            else
            {
                response.DatabaseStatus = "Disconnected";
                response.Details["Database"] = "Unavailable";
                isHealthy = false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database health check failed");
            response.DatabaseStatus = "Error";
            response.Details["Database"] = $"Error: {ex.Message}";
            isHealthy = false;
        }

        response.Status = isHealthy ? "Healthy" : "Unhealthy";

        if (isHealthy)
        {
            return Ok(response);
        }
        else
        {
            return StatusCode(503, response);
        }
    }
}
