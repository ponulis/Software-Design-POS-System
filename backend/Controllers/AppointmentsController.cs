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
    /// Supports filtering by date range, employee, service, customer, status, and payment status
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<AppointmentResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllAppointments(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int? employeeId = null,
        [FromQuery] int? serviceId = null,
        [FromQuery] string? customerName = null,
        [FromQuery] string? customerPhone = null,
        [FromQuery] string? status = null,
        [FromQuery] string? paymentStatus = null)
    {
        try
        {
            var businessIdNullable = User.GetBusinessId();
            if (!businessIdNullable.HasValue)
            {
                throw new UnauthorizedAccessException("Business ID not found in token");
            }

            var businessId = businessIdNullable.Value;
            var appointments = await _appointmentService.GetAllAppointmentsAsync(
                businessId, startDate, endDate, employeeId, serviceId, customerName, customerPhone, status, paymentStatus);
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
    /// Get available time slots for appointments
    /// </summary>
    [HttpGet("available-slots")]
    [ProducesResponseType(typeof(List<AvailableSlotResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAvailableSlots(
        [FromQuery] DateTime date,
        [FromQuery] int? employeeId = null,
        [FromQuery] int? serviceId = null,
        [FromQuery] int slotDurationMinutes = 30)
    {
        try
        {
            var businessIdNullable = User.GetBusinessId();
            if (!businessIdNullable.HasValue)
            {
                throw new UnauthorizedAccessException("Business ID not found in token");
            }

            var businessId = businessIdNullable.Value;
            var availableSlots = await _appointmentService.GetAvailableSlotsAsync(
                businessId, date, employeeId, serviceId, slotDurationMinutes);
            
            return Ok(availableSlots);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving available slots");
            return StatusCode(500, new { message = "An error occurred while retrieving available slots" });
        }
    }

    /// <summary>
    /// Cancel appointment (DELETE per API contract)
    /// </summary>
    [HttpDelete("{appointmentId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAppointment(int appointmentId, [FromBody] CancelAppointmentRequest? request = null)
    {
        try
        {
            var businessIdNullable = User.GetBusinessId();
            if (!businessIdNullable.HasValue)
            {
                throw new UnauthorizedAccessException("Business ID not found in token");
            }

            var businessId = businessIdNullable.Value;
            var cancellationReason = request?.CancellationReason;
            var notes = request?.Notes;
            
            var deleted = await _appointmentService.DeleteAppointmentAsync(appointmentId, businessId, cancellationReason, notes);

            if (!deleted)
            {
                return NotFound(new { message = "Appointment not found" });
            }

            return Ok(new { message = "Appointment cancelled successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting appointment");
            return StatusCode(500, new { message = "An error occurred while deleting the appointment" });
        }
    }
}
