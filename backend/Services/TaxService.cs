using backend.Data;
using backend.DTOs;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class TaxService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<TaxService> _logger;

    public TaxService(ApplicationDbContext context, ILogger<TaxService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<TaxResponse> CreateTaxAsync(CreateTaxRequest request, int businessId)
    {
        // Validate rate
        if (request.Rate < 0 || request.Rate > 100)
        {
            throw new InvalidOperationException("Tax rate must be between 0 and 100");
        }

        // Validate date range
        if (request.EffectiveFrom.HasValue && request.EffectiveTo.HasValue && request.EffectiveFrom.Value >= request.EffectiveTo.Value)
        {
            throw new InvalidOperationException("EffectiveFrom date must be before EffectiveTo date");
        }

        var tax = new Tax
        {
            BusinessId = businessId,
            Name = request.Name,
            Rate = request.Rate,
            IsActive = request.IsActive,
            EffectiveFrom = request.EffectiveFrom ?? DateTime.UtcNow,
            EffectiveTo = request.EffectiveTo
        };

        _context.Taxes.Add(tax);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Tax created: TaxId={TaxId}, BusinessId={BusinessId}, Name={Name}, Rate={Rate}",
            tax.Id, tax.BusinessId, tax.Name, tax.Rate);

        return MapToTaxResponse(tax);
    }

    public async Task<List<TaxResponse>> GetAllTaxesAsync(int businessId, bool? activeOnly = null)
    {
        var query = _context.Taxes
            .Where(t => t.BusinessId == businessId);

        if (activeOnly == true)
        {
            var now = DateTime.UtcNow;
            query = query.Where(t => t.IsActive
                && t.EffectiveFrom <= now
                && (t.EffectiveTo == null || t.EffectiveTo >= now));
        }

        var taxes = await query
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return taxes.Select(MapToTaxResponse).ToList();
    }

    public async Task<TaxResponse?> GetTaxByIdAsync(int taxId, int businessId)
    {
        var tax = await _context.Taxes
            .Where(t => t.Id == taxId && t.BusinessId == businessId)
            .FirstOrDefaultAsync();

        return tax != null ? MapToTaxResponse(tax) : null;
    }

    public async Task<TaxResponse?> UpdateTaxAsync(int taxId, UpdateTaxRequest request, int businessId)
    {
        var tax = await _context.Taxes
            .Where(t => t.Id == taxId && t.BusinessId == businessId)
            .FirstOrDefaultAsync();

        if (tax == null)
        {
            return null;
        }

        // Update fields
        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            tax.Name = request.Name;
        }

        if (request.Rate.HasValue)
        {
            if (request.Rate.Value < 0 || request.Rate.Value > 100)
            {
                throw new InvalidOperationException("Tax rate must be between 0 and 100");
            }

            tax.Rate = request.Rate.Value;
        }

        if (request.IsActive.HasValue)
        {
            tax.IsActive = request.IsActive.Value;
        }

        if (request.EffectiveFrom.HasValue)
        {
            tax.EffectiveFrom = request.EffectiveFrom.Value;
        }

        if (request.EffectiveTo.HasValue)
        {
            tax.EffectiveTo = request.EffectiveTo.Value;
        }

        // Validate date range
        if (tax.EffectiveTo.HasValue && tax.EffectiveFrom >= tax.EffectiveTo.Value)
        {
            throw new InvalidOperationException("EffectiveFrom date must be before EffectiveTo date");
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Tax updated: TaxId={TaxId}, BusinessId={BusinessId}, Name={Name}, Rate={Rate}",
            tax.Id, tax.BusinessId, tax.Name, tax.Rate);

        return MapToTaxResponse(tax);
    }

    public async Task<bool> DeleteTaxAsync(int taxId, int businessId)
    {
        var tax = await _context.Taxes
            .Where(t => t.Id == taxId && t.BusinessId == businessId)
            .FirstOrDefaultAsync();

        if (tax == null)
        {
            return false;
        }

        _context.Taxes.Remove(tax);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Tax deleted: TaxId={TaxId}, BusinessId={BusinessId}, Name={Name}",
            tax.Id, tax.BusinessId, tax.Name);

        return true;
    }

    private TaxResponse MapToTaxResponse(Tax tax)
    {
        return new TaxResponse
        {
            Id = tax.Id,
            BusinessId = tax.BusinessId,
            Name = tax.Name,
            Rate = tax.Rate,
            IsActive = tax.IsActive,
            EffectiveFrom = tax.EffectiveFrom,
            EffectiveTo = tax.EffectiveTo,
            CreatedAt = tax.CreatedAt
        };
    }
}
