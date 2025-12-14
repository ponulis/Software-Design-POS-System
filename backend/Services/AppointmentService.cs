using backend.Data;
using backend.DTOs;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class AppointmentService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AppointmentService> _logger;

    // Default business hours (9 AM - 5 PM)
    private static readonly TimeSpan BusinessOpenTime = new TimeSpan(9, 0, 0);
    private static readonly TimeSpan BusinessCloseTime = new TimeSpan(17, 0, 0);

    public AppointmentService(ApplicationDbContext context, ILogger<AppointmentService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<AppointmentResponse> CreateAppointmentAsync(CreateAppointmentRequest request, int businessId)
    {
        // Validate service exists and belongs to business (if provided)
        if (request.ServiceId.HasValue)
        {
            var service = await _context.Services
                .Where(s => s.Id == request.ServiceId.Value && s.BusinessId == businessId)
                .FirstOrDefaultAsync();

            if (service == null)
            {
                throw new InvalidOperationException("Service not found or doesn't belong to your business");
            }

            if (!service.Available)
            {
                throw new InvalidOperationException("Service is not available");
            }
        }

        // Validate employee exists and belongs to business (if provided)
        if (request.EmployeeId.HasValue)
        {
            var employee = await _context.Users
                .Where(u => u.Id == request.EmployeeId.Value && u.BusinessId == businessId)
                .FirstOrDefaultAsync();

            if (employee == null)
            {
                throw new InvalidOperationException("Employee not found or doesn't belong to your business");
            }
        }

        // Validate order exists and belongs to business (if provided)
        if (request.OrderId.HasValue)
        {
            var order = await _context.Orders
                .Where(o => o.Id == request.OrderId.Value && o.BusinessId == businessId)
                .FirstOrDefaultAsync();

            if (order == null)
            {
                throw new InvalidOperationException("Order not found or doesn't belong to your business");
            }
        }

        // Validate appointment date is in the future
        if (request.Date <= DateTime.UtcNow)
        {
            throw new InvalidOperationException("Appointment date must be in the future");
        }

        // Validate business hours
        ValidateBusinessHours(request.Date);

        // Validate customer name and phone
        if (string.IsNullOrWhiteSpace(request.CustomerName))
        {
            throw new InvalidOperationException("Customer name is required");
        }

        if (string.IsNullOrWhiteSpace(request.CustomerPhone))
        {
            throw new InvalidOperationException("Customer phone is required");
        }

        // Check for conflicts (overlapping appointments)
        await ValidateNoConflictsAsync(request, businessId, null);

        // Create appointment
        var appointment = new Appointment
        {
            BusinessId = businessId,
            ServiceId = request.ServiceId,
            EmployeeId = request.EmployeeId,
            Date = request.Date,
            CustomerName = request.CustomerName,
            CustomerPhone = request.CustomerPhone,
            Notes = request.Notes,
            Status = AppointmentStatus.Scheduled,
            OrderId = request.OrderId
        };

        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Appointment created: AppointmentId={AppointmentId}, BusinessId={BusinessId}, Date={Date}, CustomerName={CustomerName}",
            appointment.Id, appointment.BusinessId, appointment.Date, appointment.CustomerName);

        return await MapToAppointmentResponseAsync(appointment);
    }

    public async Task<List<AppointmentResponse>> GetAllAppointmentsAsync(int businessId, DateTime? startDate = null, DateTime? endDate = null, int? employeeId = null, int? serviceId = null)
    {
        var query = _context.Appointments
            .Include(a => a.Service)
            .Include(a => a.Employee)
            .Where(a => a.BusinessId == businessId);

        if (startDate.HasValue)
        {
            query = query.Where(a => a.Date >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(a => a.Date <= endDate.Value);
        }

        if (employeeId.HasValue)
        {
            query = query.Where(a => a.EmployeeId == employeeId.Value);
        }

        if (serviceId.HasValue)
        {
            query = query.Where(a => a.ServiceId == serviceId.Value);
        }

        var appointments = await query
            .OrderBy(a => a.Date)
            .ToListAsync();

        var responses = new List<AppointmentResponse>();
        foreach (var appointment in appointments)
        {
            responses.Add(await MapToAppointmentResponseAsync(appointment));
        }

        return responses;
    }

    public async Task<AppointmentResponse?> GetAppointmentByIdAsync(int appointmentId, int businessId)
    {
        var appointment = await _context.Appointments
            .Include(a => a.Service)
            .Include(a => a.Employee)
            .Where(a => a.Id == appointmentId && a.BusinessId == businessId)
            .FirstOrDefaultAsync();

        return appointment != null ? await MapToAppointmentResponseAsync(appointment) : null;
    }

    public async Task<AppointmentResponse?> UpdateAppointmentAsync(int appointmentId, UpdateAppointmentRequest request, int businessId)
    {
        var appointment = await _context.Appointments
            .Include(a => a.Service)
            .Where(a => a.Id == appointmentId && a.BusinessId == businessId)
            .FirstOrDefaultAsync();

        if (appointment == null)
        {
            return null;
        }

        // Cannot update cancelled or completed appointments
        if (appointment.Status == AppointmentStatus.Cancelled || appointment.Status == AppointmentStatus.Completed)
        {
            throw new InvalidOperationException($"Cannot update appointment with status {appointment.Status}");
        }

        // Validate service if provided
        if (request.ServiceId.HasValue)
        {
            var service = await _context.Services
                .Where(s => s.Id == request.ServiceId.Value && s.BusinessId == businessId)
                .FirstOrDefaultAsync();

            if (service == null)
            {
                throw new InvalidOperationException("Service not found or doesn't belong to your business");
            }

            if (!service.Available)
            {
                throw new InvalidOperationException("Service is not available");
            }

            appointment.ServiceId = request.ServiceId.Value;
        }

        // Validate employee if provided
        if (request.EmployeeId.HasValue)
        {
            var employee = await _context.Users
                .Where(u => u.Id == request.EmployeeId.Value && u.BusinessId == businessId)
                .FirstOrDefaultAsync();

            if (employee == null)
            {
                throw new InvalidOperationException("Employee not found or doesn't belong to your business");
            }

            appointment.EmployeeId = request.EmployeeId.Value;
        }

        // Validate order if provided
        if (request.OrderId.HasValue)
        {
            var order = await _context.Orders
                .Where(o => o.Id == request.OrderId.Value && o.BusinessId == businessId)
                .FirstOrDefaultAsync();

            if (order == null)
            {
                throw new InvalidOperationException("Order not found or doesn't belong to your business");
            }

            appointment.OrderId = request.OrderId.Value;
        }

        // Update date if provided (reschedule logic)
        DateTime newDate = appointment.Date;
        bool isRescheduling = false;
        if (request.Date.HasValue)
        {
            if (request.Date.Value <= DateTime.UtcNow)
            {
                throw new InvalidOperationException("Appointment date must be in the future");
            }

            ValidateBusinessHours(request.Date.Value);
            
            // Check if this is actually a reschedule (date changed)
            if (request.Date.Value != appointment.Date)
            {
                isRescheduling = true;
            }
            
            newDate = request.Date.Value;
        }

        // Check for conflicts if date, employee, or service changed
        if (request.Date.HasValue || request.EmployeeId.HasValue || request.ServiceId.HasValue)
        {
            await ValidateNoConflictsAsync(
                new CreateAppointmentRequest
                {
                    ServiceId = request.ServiceId ?? appointment.ServiceId,
                    EmployeeId = request.EmployeeId ?? appointment.EmployeeId,
                    Date = newDate
                },
                businessId,
                appointmentId); // Exclude current appointment from conflict check
        }

        // Update fields
        if (request.Date.HasValue)
        {
            appointment.Date = request.Date.Value;
        }
        
        // If rescheduling and status not explicitly set, mark as Rescheduled
        if (isRescheduling && string.IsNullOrWhiteSpace(request.Status))
        {
            appointment.Status = AppointmentStatus.Rescheduled;
        }

        if (!string.IsNullOrWhiteSpace(request.CustomerName))
        {
            appointment.CustomerName = request.CustomerName;
        }

        if (!string.IsNullOrWhiteSpace(request.CustomerPhone))
        {
            appointment.CustomerPhone = request.CustomerPhone;
        }

        if (request.Notes != null)
        {
            appointment.Notes = request.Notes;
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            if (Enum.TryParse<AppointmentStatus>(request.Status, true, out var status))
            {
                appointment.Status = status;
            }
            else
            {
                throw new InvalidOperationException($"Invalid appointment status: {request.Status}");
            }
        }

        // Track activity log: Update UpdatedAt timestamp
        appointment.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Activity log: Log appointment changes
        var changeDetails = new List<string>();
        if (isRescheduling)
        {
            changeDetails.Add($"Rescheduled to {appointment.Date:yyyy-MM-dd HH:mm}");
        }
        if (request.EmployeeId.HasValue)
        {
            changeDetails.Add($"Employee changed");
        }
        if (request.ServiceId.HasValue)
        {
            changeDetails.Add($"Service changed");
        }
        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            changeDetails.Add($"Status changed to {appointment.Status}");
        }

        _logger.LogInformation(
            "Appointment updated: AppointmentId={AppointmentId}, BusinessId={BusinessId}, Date={Date}, Status={Status}, Changes={Changes}, UpdatedAt={UpdatedAt}",
            appointment.Id, appointment.BusinessId, appointment.Date, appointment.Status, 
            string.Join(", ", changeDetails), appointment.UpdatedAt);

        return await MapToAppointmentResponseAsync(appointment);
    }

    public async Task<bool> DeleteAppointmentAsync(int appointmentId, int businessId)
    {
        var appointment = await _context.Appointments
            .Where(a => a.Id == appointmentId && a.BusinessId == businessId)
            .FirstOrDefaultAsync();

        if (appointment == null)
        {
            return false;
        }

        // Instead of deleting, mark as cancelled
        appointment.Status = AppointmentStatus.Cancelled;
        appointment.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Appointment cancelled: AppointmentId={AppointmentId}, BusinessId={BusinessId}, Date={Date}",
            appointment.Id, appointment.BusinessId, appointment.Date);

        return true;
    }

    private void ValidateBusinessHours(DateTime appointmentDate)
    {
        var appointmentTime = appointmentDate.TimeOfDay;

        if (appointmentTime < BusinessOpenTime || appointmentTime > BusinessCloseTime)
        {
            throw new InvalidOperationException($"Appointment time must be between {BusinessOpenTime:hh\\:mm} and {BusinessCloseTime:hh\\:mm}");
        }
    }

    private async Task ValidateNoConflictsAsync(CreateAppointmentRequest request, int businessId, int? excludeAppointmentId)
    {
        // Get service duration if service is provided
        int? serviceDurationMinutes = null;
        if (request.ServiceId.HasValue)
        {
            var service = await _context.Services
                .Where(s => s.Id == request.ServiceId.Value)
                .FirstOrDefaultAsync();

            if (service != null)
            {
                serviceDurationMinutes = service.DurationMinutes;
            }
        }

        // Default duration to 30 minutes if not specified
        int durationMinutes = serviceDurationMinutes ?? 30;
        var appointmentEndTime = request.Date.AddMinutes(durationMinutes);

        // Check for overlapping appointments
        // Load appointments with their services to calculate end times
        var existingAppointments = await _context.Appointments
            .Include(a => a.Service)
            .Where(a => a.BusinessId == businessId &&
                       a.Status != AppointmentStatus.Cancelled)
            .ToListAsync();

        var conflictingAppointments = existingAppointments
            .Where(a =>
            {
                // Exclude current appointment if updating
                if (excludeAppointmentId.HasValue && a.Id == excludeAppointmentId.Value)
                {
                    return false;
                }

                // If employee is specified, check conflicts for that employee
                if (request.EmployeeId.HasValue && a.EmployeeId != request.EmployeeId.Value)
                {
                    return false;
                }

                // If service is specified but no employee, check conflicts for that service
                if (!request.EmployeeId.HasValue && request.ServiceId.HasValue && a.ServiceId != request.ServiceId.Value)
                {
                    return false;
                }

                // Calculate appointment end times
                var existingAppointmentDuration = a.Service?.DurationMinutes ?? 30;
                var existingAppointmentEndTime = a.Date.AddMinutes(existingAppointmentDuration);

                // Check for overlap
                return a.Date < appointmentEndTime && request.Date < existingAppointmentEndTime;
            })
            .ToList();

        if (conflictingAppointments.Any())
        {
            var conflict = conflictingAppointments.First();
            throw new InvalidOperationException(
                $"Appointment conflicts with existing appointment on {conflict.Date:yyyy-MM-dd HH:mm}. " +
                $"Please choose a different time slot.");
        }
    }

    /// <summary>
    /// Get available time slots for appointments
    /// </summary>
    public async Task<List<AvailableSlotResponse>> GetAvailableSlotsAsync(
        int businessId,
        DateTime date,
        int? employeeId = null,
        int? serviceId = null,
        int slotDurationMinutes = 30)
    {
        // Validate date is in the future
        if (date.Date < DateTime.UtcNow.Date)
        {
            throw new InvalidOperationException("Date must be today or in the future");
        }

        // Get service duration if service is provided
        int appointmentDurationMinutes = slotDurationMinutes;
        if (serviceId.HasValue)
        {
            var service = await _context.Services
                .Where(s => s.Id == serviceId.Value && s.BusinessId == businessId)
                .FirstOrDefaultAsync();

            if (service == null)
            {
                throw new InvalidOperationException("Service not found or doesn't belong to your business");
            }

            if (!service.Available)
            {
                throw new InvalidOperationException("Service is not available");
            }

            appointmentDurationMinutes = service.DurationMinutes;
        }

        // Validate employee if provided
        if (employeeId.HasValue)
        {
            var employee = await _context.Users
                .Where(u => u.Id == employeeId.Value && u.BusinessId == businessId)
                .FirstOrDefaultAsync();

            if (employee == null)
            {
                throw new InvalidOperationException("Employee not found or doesn't belong to your business");
            }
        }

        // Get all existing appointments for the date
        var startOfDay = date.Date.Add(BusinessOpenTime);
        var endOfDay = date.Date.Add(BusinessCloseTime);

        var existingAppointments = await _context.Appointments
            .Include(a => a.Service)
            .Include(a => a.Employee)
            .Where(a => a.BusinessId == businessId &&
                       a.Date.Date == date.Date &&
                       a.Status != AppointmentStatus.Cancelled)
            .ToListAsync();

        // Filter by employee if specified
        if (employeeId.HasValue)
        {
            existingAppointments = existingAppointments
                .Where(a => a.EmployeeId == employeeId.Value)
                .ToList();
        }

        // Generate available slots
        var availableSlots = new List<AvailableSlotResponse>();
        var currentTime = startOfDay;

        while (currentTime.AddMinutes(appointmentDurationMinutes) <= endOfDay)
        {
            var slotEndTime = currentTime.AddMinutes(appointmentDurationMinutes);
            bool isAvailable = true;

            // Check if this slot conflicts with any existing appointment
            foreach (var appointment in existingAppointments)
            {
                var appointmentDuration = appointment.Service?.DurationMinutes ?? 30;
                var appointmentEndTime = appointment.Date.AddMinutes(appointmentDuration);

                // Check for overlap
                if (currentTime < appointmentEndTime && appointment.Date < slotEndTime)
                {
                    isAvailable = false;
                    break;
                }
            }

            if (isAvailable)
            {
                // Get employee name if employeeId is specified
                string? employeeName = null;
                if (employeeId.HasValue)
                {
                    var employee = await _context.Users
                        .Where(u => u.Id == employeeId.Value)
                        .FirstOrDefaultAsync();
                    employeeName = employee?.Name;
                }

                // Get service name if serviceId is specified
                string? serviceName = null;
                if (serviceId.HasValue)
                {
                    var service = await _context.Services
                        .Where(s => s.Id == serviceId.Value)
                        .FirstOrDefaultAsync();
                    serviceName = service?.Name;
                }

                availableSlots.Add(new AvailableSlotResponse
                {
                    StartTime = currentTime,
                    EndTime = slotEndTime,
                    EmployeeId = employeeId,
                    EmployeeName = employeeName,
                    ServiceId = serviceId,
                    ServiceName = serviceName
                });
            }

            // Move to next slot (increment by slot duration)
            currentTime = currentTime.AddMinutes(slotDurationMinutes);
        }

        return availableSlots;
    }

    private async Task<AppointmentResponse> MapToAppointmentResponseAsync(Appointment appointment)
    {
        // Reload with includes if not already loaded
        if (appointment.Service == null || appointment.Employee == null)
        {
            appointment = await _context.Appointments
                .Include(a => a.Service)
                .Include(a => a.Employee)
                .Where(a => a.Id == appointment.Id)
                .FirstAsync();
        }

        return new AppointmentResponse
        {
            Id = appointment.Id,
            BusinessId = appointment.BusinessId,
            ServiceId = appointment.ServiceId,
            ServiceName = appointment.Service?.Name,
            EmployeeId = appointment.EmployeeId,
            EmployeeName = appointment.Employee?.Name,
            Date = appointment.Date,
            CustomerName = appointment.CustomerName,
            CustomerPhone = appointment.CustomerPhone,
            Notes = appointment.Notes,
            Status = appointment.Status.ToString(),
            OrderId = appointment.OrderId,
            CreatedAt = appointment.CreatedAt,
            UpdatedAt = appointment.UpdatedAt
        };
    }
}
