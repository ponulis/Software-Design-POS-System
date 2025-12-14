using backend.Data;
using backend.DTOs;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class DiscountService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DiscountService> _logger;

    public DiscountService(ApplicationDbContext context, ILogger<DiscountService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<DiscountResponse> CreateDiscountAsync(CreateDiscountRequest request, int businessId)
    {
        // Validate discount type
        if (!Enum.TryParse<DiscountType>(request.Type, true, out var discountType))
        {
            throw new InvalidOperationException($"Invalid discount type: {request.Type}. Valid types are: Percentage, FixedAmount");
        }

        // Validate value based on type
        if (discountType == DiscountType.Percentage && (request.Value < 0 || request.Value > 100))
        {
            throw new InvalidOperationException("Percentage discount value must be between 0 and 100");
        }

        if (discountType == DiscountType.FixedAmount && request.Value <= 0)
        {
            throw new InvalidOperationException("Fixed amount discount value must be greater than 0");
        }

        // Validate date range
        if (request.ValidFrom.HasValue && request.ValidTo.HasValue && request.ValidFrom.Value >= request.ValidTo.Value)
        {
            throw new InvalidOperationException("ValidFrom date must be before ValidTo date");
        }

        var discount = new Discount
        {
            BusinessId = businessId,
            Name = request.Name,
            Description = request.Description,
            Type = discountType,
            Value = request.Value,
            IsActive = request.IsActive,
            ValidFrom = request.ValidFrom,
            ValidTo = request.ValidTo
        };

        _context.Discounts.Add(discount);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Discount created: DiscountId={DiscountId}, BusinessId={BusinessId}, Name={Name}, Type={Type}, Value={Value}",
            discount.Id, discount.BusinessId, discount.Name, discount.Type, discount.Value);

        return MapToDiscountResponse(discount);
    }

    public async Task<List<DiscountResponse>> GetAllDiscountsAsync(int businessId, bool? activeOnly = null)
    {
        var query = _context.Discounts
            .Where(d => d.BusinessId == businessId);

        if (activeOnly == true)
        {
            var now = DateTime.UtcNow;
            query = query.Where(d => d.IsActive
                && (d.ValidFrom == null || d.ValidFrom <= now)
                && (d.ValidTo == null || d.ValidTo >= now));
        }

        var discounts = await query
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();

        return discounts.Select(MapToDiscountResponse).ToList();
    }

    public async Task<DiscountResponse?> GetDiscountByIdAsync(int discountId, int businessId)
    {
        var discount = await _context.Discounts
            .Where(d => d.Id == discountId && d.BusinessId == businessId)
            .FirstOrDefaultAsync();

        return discount != null ? MapToDiscountResponse(discount) : null;
    }

    public async Task<DiscountResponse?> UpdateDiscountAsync(int discountId, UpdateDiscountRequest request, int businessId)
    {
        var discount = await _context.Discounts
            .Where(d => d.Id == discountId && d.BusinessId == businessId)
            .FirstOrDefaultAsync();

        if (discount == null)
        {
            return null;
        }

        // Update fields
        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            discount.Name = request.Name;
        }

        if (request.Description != null)
        {
            discount.Description = request.Description;
        }

        if (!string.IsNullOrWhiteSpace(request.Type))
        {
            if (Enum.TryParse<DiscountType>(request.Type, true, out var discountType))
            {
                discount.Type = discountType;
            }
            else
            {
                throw new InvalidOperationException($"Invalid discount type: {request.Type}");
            }
        }

        if (request.Value.HasValue)
        {
            // Validate value based on type
            var discountType = discount.Type;
            if (discountType == DiscountType.Percentage && (request.Value.Value < 0 || request.Value.Value > 100))
            {
                throw new InvalidOperationException("Percentage discount value must be between 0 and 100");
            }

            if (discountType == DiscountType.FixedAmount && request.Value.Value <= 0)
            {
                throw new InvalidOperationException("Fixed amount discount value must be greater than 0");
            }

            discount.Value = request.Value.Value;
        }

        if (request.IsActive.HasValue)
        {
            discount.IsActive = request.IsActive.Value;
        }

        if (request.ValidFrom.HasValue)
        {
            discount.ValidFrom = request.ValidFrom.Value;
        }

        if (request.ValidTo.HasValue)
        {
            discount.ValidTo = request.ValidTo.Value;
        }

        // Validate date range
        if (discount.ValidFrom.HasValue && discount.ValidTo.HasValue && discount.ValidFrom.Value >= discount.ValidTo.Value)
        {
            throw new InvalidOperationException("ValidFrom date must be before ValidTo date");
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Discount updated: DiscountId={DiscountId}, BusinessId={BusinessId}, Name={Name}, Type={Type}, Value={Value}",
            discount.Id, discount.BusinessId, discount.Name, discount.Type, discount.Value);

        return MapToDiscountResponse(discount);
    }

    public async Task<bool> DeleteDiscountAsync(int discountId, int businessId)
    {
        var discount = await _context.Discounts
            .Where(d => d.Id == discountId && d.BusinessId == businessId)
            .FirstOrDefaultAsync();

        if (discount == null)
        {
            return false;
        }

        _context.Discounts.Remove(discount);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Discount deleted: DiscountId={DiscountId}, BusinessId={BusinessId}, Name={Name}",
            discount.Id, discount.BusinessId, discount.Name);

        return true;
    }

    private DiscountResponse MapToDiscountResponse(Discount discount)
    {
        return new DiscountResponse
        {
            Id = discount.Id,
            BusinessId = discount.BusinessId,
            Name = discount.Name,
            Description = discount.Description,
            Type = discount.Type.ToString(),
            Value = discount.Value,
            IsActive = discount.IsActive,
            ValidFrom = discount.ValidFrom,
            ValidTo = discount.ValidTo,
            CreatedAt = discount.CreatedAt
        };
    }
}
