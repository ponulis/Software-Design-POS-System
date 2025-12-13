using backend.DTOs;
using backend.Extensions;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/appointments")]
[Authorize]
public class AppointmentsController : ControllerBase
{
    private readonly AppointmentService _appointmentService;
    private readonly ILogger<AppointmentsController> _logger;

    public AppointmentsController(AppointmentService appointmentService, ILogger<AppointmentsController> logger)
    {
        _appointmentService = appointmentService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new appointment
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(AppointmentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateAppointment([FromBody] CreateAppointmentRequest request)
    {
        try
        {
            var businessIdNullable = User.GetBusinessId();
            if (!businessIdNullable.HasValue)
            {
                throw new UnauthorizedAccessException("Business ID not found in token");
            }

            var businessId = businessIdNullable.Value;
            var appointment = await _appointmentService.CreateAppointmentAsync(request, businessId);
            return CreatedAtAction(nameof(GetAppointmentById), new { appointmentId = appointment.Id }, appointment);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating appointment");
            return StatusCode(500, new { message = "An error occurred while creating the appointment" });
        }
    }

    /// <summary>
    /// Get all appointments for the authenticated user's business
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<AppointmentResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllAppointments(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int? employeeId = null,
        [FromQuery] int? serviceId = null)
    {
        try
        {
            var businessIdNullable = User.GetBusinessId();
            if (!businessIdNullable.HasValue)
            {
                throw new UnauthorizedAccessException("Business ID not found in token");
            }

            var businessId = businessIdNullable.Value;
            var appointments = await _appointmentService.GetAllAppointmentsAsync(businessId, startDate, endDate, employeeId, serviceId);
            return Ok(appointments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving appointments");
            return StatusCode(500, new { message = "An error occurred while retrieving appointments" });
        }
    }

    /// <summary>
    /// Get appointment by ID
    /// </summary>
    [HttpGet("{appointmentId}")]
    [ProducesResponseType(typeof(AppointmentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAppointmentById(int appointmentId)
    {
        try
        {
            var businessIdNullable = User.GetBusinessId();
            if (!businessIdNullable.HasValue)
            {
                throw new UnauthorizedAccessException("Business ID not found in token");
            }

            var businessId = businessIdNullable.Value;
            var appointment = await _appointmentService.GetAppointmentByIdAsync(appointmentId, businessId);

            if (appointment == null)
            {
                return NotFound(new { message = "Appointment not found" });
            }

            return Ok(appointment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving appointment");
            return StatusCode(500, new { message = "An error occurred while retrieving the appointment" });
        }
    }

    /// <summary>
    /// Modify/reschedule appointment (PATCH per API contract)
    /// </summary>
    [HttpPatch("{appointmentId}")]
    [ProducesResponseType(typeof(AppointmentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAppointment(int appointmentId, [FromBody] UpdateAppointmentRequest request)
    {
        try
        {
            var businessIdNullable = User.GetBusinessId();
            if (!businessIdNullable.HasValue)
            {
                throw new UnauthorizedAccessException("Business ID not found in token");
            }

            var businessId = businessIdNullable.Value;
            var appointment = await _appointmentService.UpdateAppointmentAsync(appointmentId, request, businessId);

            if (appointment == null)
            {
                return NotFound(new { message = "Appointment not found" });
            }

            return Ok(appointment);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating appointment");
            return StatusCode(500, new { message = "An error occurred while updating the appointment" });
        }
    }

    /// <summary>
    /// Cancel appointment (DELETE per API contract)
    /// </summary>
    [HttpDelete("{appointmentId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAppointment(int appointmentId)
    {
        try
        {
            var businessIdNullable = User.GetBusinessId();
            if (!businessIdNullable.HasValue)
            {
                throw new UnauthorizedAccessException("Business ID not found in token");
            }

            var businessId = businessIdNullable.Value;
            var deleted = await _appointmentService.DeleteAppointmentAsync(appointmentId, businessId);

            if (!deleted)
            {
                return NotFound(new { message = "Appointment not found" });
            }

            return Ok(new { message = "Appointment cancelled successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting appointment");
            return StatusCode(500, new { message = "An error occurred while deleting the appointment" });
        }
    }
}
