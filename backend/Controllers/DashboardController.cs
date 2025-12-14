using backend.DTOs;
using backend.Extensions;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly AnalyticsService _analyticsService;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(AnalyticsService analyticsService, ILogger<DashboardController> logger)
    {
        _analyticsService = analyticsService;
        _logger = logger;
    }

    /// <summary>
    /// Get dashboard statistics for the authenticated user's business
    /// </summary>
    /// <param name="startDate">Optional start date for filtering (ISO 8601 format)</param>
    /// <param name="endDate">Optional end date for filtering (ISO 8601 format)</param>
    /// <returns>Dashboard data including revenue, orders, payments, and appointments statistics</returns>
    /// <remarks>
    /// Sample request:
    /// 
    ///     GET /api/dashboard
    ///     GET /api/dashboard?startDate=2024-01-01T00:00:00Z&amp;endDate=2024-12-31T23:59:59Z
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(DashboardResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboard([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var businessIdNullable = User.GetBusinessId();
            if (!businessIdNullable.HasValue)
            {
                throw new UnauthorizedAccessException("Business ID not found in token");
            }

            var businessId = businessIdNullable.Value;
            var dashboard = await _analyticsService.GetDashboardDataAsync(businessId, startDate, endDate);
            return Ok(dashboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dashboard data");
            return StatusCode(500, new { message = "An error occurred while retrieving dashboard data" });
        }
    }
}
