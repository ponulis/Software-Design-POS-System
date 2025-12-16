using backend.Data;
using backend.DTOs;
using backend.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace backend.Services;

public class ProductModificationService
{
    private readonly ApplicationDbContext _context;

    public ProductModificationService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProductModificationResponse>> GetAllModificationsAsync(int businessId)
    {
        try
        {
            var modifications = await _context.ProductModifications
                .Where(m => m.BusinessId == businessId)
                .Include(m => m.Values)
                .OrderBy(m => m.Name)
                .ToListAsync();

            return modifications.Select(MapToResponse).ToList();
        }
        catch (Microsoft.Data.SqlClient.SqlException sqlEx) when (sqlEx.Message.Contains("Invalid object name"))
        {
            // Tables don't exist yet - return empty list
            return new List<ProductModificationResponse>();
        }
    }

    public async Task<ProductModificationResponse?> GetModificationByIdAsync(int modificationId, int businessId)
    {
        var modification = await _context.ProductModifications
            .Where(m => m.Id == modificationId && m.BusinessId == businessId)
            .Include(m => m.Values)
            .FirstOrDefaultAsync();

        return modification != null ? MapToResponse(modification) : null;
    }

    public async Task<ProductModificationResponse> CreateModificationAsync(CreateProductModificationRequest request, int businessId)
    {
        // Check if modification with same name already exists for this business
        var existing = await _context.ProductModifications
            .Where(m => m.BusinessId == businessId && m.Name.ToLower() == request.Name.ToLower())
            .FirstOrDefaultAsync();

        if (existing != null)
        {
            throw new InvalidOperationException($"Modification '{request.Name}' already exists for this business");
        }

        // Parse price type
        PriceModificationType priceType = PriceModificationType.None;
        if (!string.IsNullOrEmpty(request.PriceType))
        {
            if (Enum.TryParse<PriceModificationType>(request.PriceType, true, out var parsedType))
            {
                priceType = parsedType;
            }
        }

        // Validate pricing based on type
        if (priceType == PriceModificationType.Fixed && (!request.FixedPriceAddition.HasValue || request.FixedPriceAddition.Value < 0))
        {
            throw new InvalidOperationException("Fixed price addition must be specified and non-negative when PriceType is Fixed");
        }

        if (priceType == PriceModificationType.Percentage && (!request.PercentagePriceIncrease.HasValue || request.PercentagePriceIncrease.Value < 0 || request.PercentagePriceIncrease.Value > 100))
        {
            throw new InvalidOperationException("Percentage price increase must be between 0 and 100 when PriceType is Percentage");
        }

        var modification = new ProductModification
        {
            BusinessId = businessId,
            Name = request.Name,
            PriceType = priceType,
            FixedPriceAddition = priceType == PriceModificationType.Fixed ? request.FixedPriceAddition : null,
            PercentagePriceIncrease = priceType == PriceModificationType.Percentage ? request.PercentagePriceIncrease : null,
            CreatedAt = DateTime.UtcNow
        };

        _context.ProductModifications.Add(modification);

        // Add initial values if provided
        if (request.Values != null && request.Values.Any())
        {
            foreach (var valueName in request.Values)
            {
                var value = new ProductModificationValue
                {
                    ModificationId = modification.Id,
                    Value = valueName.Trim(),
                    CreatedAt = DateTime.UtcNow
                };
                modification.Values.Add(value);
            }
        }

        await _context.SaveChangesAsync();

        // Reload with values
        await _context.Entry(modification)
            .Collection(m => m.Values)
            .LoadAsync();

        return MapToResponse(modification);
    }

    public async Task<ProductModificationValue> AddValueToModificationAsync(int modificationId, string valueName, int businessId)
    {
        var modification = await _context.ProductModifications
            .Where(m => m.Id == modificationId && m.BusinessId == businessId)
            .FirstOrDefaultAsync();

        if (modification == null)
        {
            throw new InvalidOperationException("Modification not found");
        }

        // Check if value already exists
        var existingValue = await _context.ProductModificationValues
            .Where(v => v.ModificationId == modificationId && v.Value.ToLower() == valueName.ToLower())
            .FirstOrDefaultAsync();

        if (existingValue != null)
        {
            throw new InvalidOperationException($"Value '{valueName}' already exists for modification '{modification.Name}'");
        }

        var value = new ProductModificationValue
        {
            ModificationId = modificationId,
            Value = valueName.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _context.ProductModificationValues.Add(value);
        await _context.SaveChangesAsync();

        return value;
    }

    public async Task<bool> DeleteModificationAsync(int modificationId, int businessId)
    {
        var modification = await _context.ProductModifications
            .Where(m => m.Id == modificationId && m.BusinessId == businessId)
            .Include(m => m.ProductAssignments)
            .FirstOrDefaultAsync();

        if (modification == null)
        {
            return false;
        }

        // Check if modification is used by any products
        if (modification.ProductAssignments.Any())
        {
            throw new InvalidOperationException($"Cannot delete modification '{modification.Name}' because it is assigned to one or more products");
        }

        _context.ProductModifications.Remove(modification);
        await _context.SaveChangesAsync();

        return true;
    }

    private ProductModificationResponse MapToResponse(ProductModification modification)
    {
        return new ProductModificationResponse
        {
            Id = modification.Id,
            Name = modification.Name,
            Values = modification.Values.Select(v => new ProductModificationValueResponse
            {
                Id = v.Id,
                Value = v.Value,
                CreatedAt = v.CreatedAt
            }).ToList(),
            PriceType = modification.PriceType.ToString(),
            FixedPriceAddition = modification.FixedPriceAddition,
            PercentagePriceIncrease = modification.PercentagePriceIncrease,
            CreatedAt = modification.CreatedAt,
            UpdatedAt = modification.UpdatedAt
        };
    }
}
