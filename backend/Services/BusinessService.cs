using backend.Data;
using backend.DTOs;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class BusinessService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<BusinessService> _logger;

    public BusinessService(ApplicationDbContext context, ILogger<BusinessService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<BusinessResponse?> GetBusinessByIdAsync(int businessId)
    {
        var business = await _context.Businesses
            .Where(b => b.Id == businessId)
            .FirstOrDefaultAsync();

        return business != null ? MapToBusinessResponse(business) : null;
    }

    public async Task<BusinessResponse?> UpdateBusinessAsync(int businessId, UpdateBusinessRequest request)
    {
        var business = await _context.Businesses
            .Where(b => b.Id == businessId)
            .FirstOrDefaultAsync();

        if (business == null)
        {
            return null;
        }

        // Update fields if provided
        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            business.Name = request.Name;
        }

        if (request.Description != null) // Allow empty string
        {
            business.Description = request.Description;
        }

        if (!string.IsNullOrWhiteSpace(request.Address))
        {
            business.Address = request.Address;
        }

        if (!string.IsNullOrWhiteSpace(request.ContactEmail))
        {
            business.ContactEmail = request.ContactEmail;
        }

        if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
        {
            business.PhoneNumber = request.PhoneNumber;
        }

        business.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Business updated: BusinessId={BusinessId}, Name={Name}",
            business.Id, business.Name);

        return MapToBusinessResponse(business);
    }

    private BusinessResponse MapToBusinessResponse(Business business)
    {
        return new BusinessResponse
        {
            Id = business.Id,
            OwnerId = business.OwnerId,
            Name = business.Name,
            Description = business.Description,
            Address = business.Address,
            ContactEmail = business.ContactEmail,
            PhoneNumber = business.PhoneNumber,
            CreatedAt = business.CreatedAt,
            UpdatedAt = business.UpdatedAt
        };
    }
}
