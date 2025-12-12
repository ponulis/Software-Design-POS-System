using backend.Data;
using backend.DTOs;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class ServiceService
{
    private readonly ApplicationDbContext _context;

    public ServiceService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ServiceResponse>> GetAllServicesAsync(int businessId, bool? availableOnly = null)
    {
        var query = _context.Services
            .Where(s => s.BusinessId == businessId);

        if (availableOnly == true)
        {
            query = query.Where(s => s.Available);
        }

        var services = await query
            .OrderBy(s => s.Name)
            .ToListAsync();

        return services.Select(MapToServiceResponse).ToList();
    }

    public async Task<ServiceResponse?> GetServiceByIdAsync(int serviceId, int businessId)
    {
        var service = await _context.Services
            .Where(s => s.Id == serviceId && s.BusinessId == businessId)
            .FirstOrDefaultAsync();

        return service != null ? MapToServiceResponse(service) : null;
    }

    public async Task<ServiceResponse> CreateServiceAsync(CreateServiceRequest request, int businessId)
    {
        var service = new Service
        {
            BusinessId = businessId,
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            DurationMinutes = request.DurationMinutes,
            Available = request.Available,
            CreatedAt = DateTime.UtcNow
        };

        _context.Services.Add(service);
        await _context.SaveChangesAsync();

        return MapToServiceResponse(service);
    }

    public async Task<ServiceResponse?> UpdateServiceAsync(int serviceId, UpdateServiceRequest request, int businessId)
    {
        var service = await _context.Services
            .Where(s => s.Id == serviceId && s.BusinessId == businessId)
            .FirstOrDefaultAsync();

        if (service == null)
        {
            return null;
        }

        if (!string.IsNullOrEmpty(request.Name))
        {
            service.Name = request.Name;
        }

        if (request.Description != null)
        {
            service.Description = request.Description;
        }

        if (request.Price.HasValue)
        {
            service.Price = request.Price.Value;
        }

        if (request.DurationMinutes.HasValue)
        {
            service.DurationMinutes = request.DurationMinutes.Value;
        }

        if (request.Available.HasValue)
        {
            service.Available = request.Available.Value;
        }

        service.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapToServiceResponse(service);
    }

    public async Task<bool> DeleteServiceAsync(int serviceId, int businessId)
    {
        var service = await _context.Services
            .Where(s => s.Id == serviceId && s.BusinessId == businessId)
            .FirstOrDefaultAsync();

        if (service == null)
        {
            return false;
        }

        // Check if service is used in any appointments
        var hasAppointments = await _context.Appointments
            .AnyAsync(a => a.ServiceId == serviceId);

        if (hasAppointments)
        {
            throw new InvalidOperationException("Cannot delete service that is used in appointments. Set Available to false instead.");
        }

        _context.Services.Remove(service);
        await _context.SaveChangesAsync();

        return true;
    }

    private ServiceResponse MapToServiceResponse(Service service)
    {
        return new ServiceResponse
        {
            Id = service.Id,
            Name = service.Name,
            Description = service.Description,
            Price = service.Price,
            DurationMinutes = service.DurationMinutes,
            Available = service.Available,
            CreatedAt = service.CreatedAt,
            UpdatedAt = service.UpdatedAt
        };
    }
}
