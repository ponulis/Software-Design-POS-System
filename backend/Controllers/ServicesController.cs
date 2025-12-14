using backend.DTOs;
using backend.Extensions;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/services")]
[Authorize]
public class ServicesController : ControllerBase
{
    private readonly ServiceService _serviceService;
    private readonly ILogger<ServicesController> _logger;

    public ServicesController(ServiceService serviceService, ILogger<ServicesController> logger)
    {
        _serviceService = serviceService;
        _logger = logger;
    }

    /// <summary>
    /// Get all services for the authenticated user's business
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<ServiceResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllServices([FromQuery] bool? availableOnly = null)
    {
        try
        {
            var businessIdNullable = User.GetBusinessId();
            if (!businessIdNullable.HasValue)
            {
                throw new UnauthorizedAccessException("Business ID not found in token");
            }

            var businessId = businessIdNullable.Value;
            var services = await _serviceService.GetAllServicesAsync(businessId, availableOnly);
            return Ok(services);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving services");
            return StatusCode(500, new { message = "An error occurred while retrieving services" });
        }
    }

    /// <summary>
    /// Get service by ID
    /// </summary>
    [HttpGet("{serviceId}")]
    [ProducesResponseType(typeof(ServiceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetServiceById(int serviceId)
    {
        try
        {
            var businessIdNullable = User.GetBusinessId();
            if (!businessIdNullable.HasValue)
            {
                throw new UnauthorizedAccessException("Business ID not found in token");
            }

            var businessId = businessIdNullable.Value;
            var service = await _serviceService.GetServiceByIdAsync(serviceId, businessId);

            if (service == null)
            {
                return NotFound(new { message = "Service not found" });
            }

            return Ok(service);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving service");
            return StatusCode(500, new { message = "An error occurred while retrieving the service" });
        }
    }

    /// <summary>
    /// Create a new service
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Manager,Admin")]
    [ProducesResponseType(typeof(ServiceResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateService([FromBody] CreateServiceRequest request)
    {
        try
        {
            var businessIdNullable = User.GetBusinessId();
            if (!businessIdNullable.HasValue)
            {
                throw new UnauthorizedAccessException("Business ID not found in token");
            }

            var businessId = businessIdNullable.Value;

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest(new { message = "Name is required" });
            }

            if (request.Price < 0)
            {
                return BadRequest(new { message = "Price must be non-negative" });
            }

            if (request.DurationMinutes <= 0)
            {
                return BadRequest(new { message = "Duration must be greater than 0" });
            }

            var service = await _serviceService.CreateServiceAsync(request, businessId);
            return CreatedAtAction(nameof(GetServiceById), new { serviceId = service.Id }, service);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating service");
            return StatusCode(500, new { message = "An error occurred while creating the service" });
        }
    }

    /// <summary>
    /// Update a service
    /// </summary>
    [HttpPatch("{serviceId}")]
    [Authorize(Roles = "Manager,Admin")]
    [ProducesResponseType(typeof(ServiceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateService(int serviceId, [FromBody] UpdateServiceRequest request)
    {
        try
        {
            var businessIdNullable = User.GetBusinessId();
            if (!businessIdNullable.HasValue)
            {
                throw new UnauthorizedAccessException("Business ID not found in token");
            }

            var businessId = businessIdNullable.Value;

            if (request.Price.HasValue && request.Price.Value < 0)
            {
                return BadRequest(new { message = "Price must be non-negative" });
            }

            if (request.DurationMinutes.HasValue && request.DurationMinutes.Value <= 0)
            {
                return BadRequest(new { message = "Duration must be greater than 0" });
            }

            var service = await _serviceService.UpdateServiceAsync(serviceId, request, businessId);

            if (service == null)
            {
                return NotFound(new { message = "Service not found" });
            }

            return Ok(service);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating service");
            return StatusCode(500, new { message = "An error occurred while updating the service" });
        }
    }

    /// <summary>
    /// Delete a service
    /// </summary>
    [HttpDelete("{serviceId}")]
    [Authorize(Roles = "Manager,Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteService(int serviceId)
    {
        try
        {
            var businessIdNullable = User.GetBusinessId();
            if (!businessIdNullable.HasValue)
            {
                throw new UnauthorizedAccessException("Business ID not found in token");
            }

            var businessId = businessIdNullable.Value;
            var deleted = await _serviceService.DeleteServiceAsync(serviceId, businessId);

            if (!deleted)
            {
                return NotFound(new { message = "Service not found" });
            }

            return Ok(new { message = "Service deleted successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting service");
            return StatusCode(500, new { message = "An error occurred while deleting the service" });
        }
    }
}
