using backend.Data;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class PricingService
{
    private readonly ApplicationDbContext _context;

    public PricingService(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Calculate subtotal from order items
    /// </summary>
    public decimal CalculateSubtotal(Order order)
    {
        return order.Items.Sum(i => i.Price * i.Quantity);
    }

    /// <summary>
    /// Calculate tax amount based on active tax rates for the business
    /// </summary>
    public async Task<decimal> CalculateTaxAsync(decimal subtotal, int businessId)
    {
        var now = DateTime.UtcNow;
        var activeTaxes = await _context.Taxes
            .Where(t => t.BusinessId == businessId 
                && t.IsActive 
                && t.EffectiveFrom <= now
                && (t.EffectiveTo == null || t.EffectiveTo >= now))
            .ToListAsync();

        if (!activeTaxes.Any())
        {
            return 0;
        }

        // Sum all active tax rates and apply to subtotal
        var totalTaxRate = activeTaxes.Sum(t => t.Rate);
        return subtotal * (totalTaxRate / 100);
    }

    /// <summary>
    /// Apply discount to subtotal based on active discounts for the business
    /// </summary>
    public async Task<decimal> ApplyDiscountAsync(decimal subtotal, int businessId, int? discountId = null)
    {
        var now = DateTime.UtcNow;
        Discount? discount = null;

        if (discountId.HasValue)
        {
            // Apply specific discount if provided
            discount = await _context.Discounts
                .Where(d => d.Id == discountId.Value 
                    && d.BusinessId == businessId 
                    && d.IsActive
                    && (d.ValidFrom == null || d.ValidFrom <= now)
                    && (d.ValidTo == null || d.ValidTo >= now))
                .FirstOrDefaultAsync();
        }
        else
        {
            // Get the first active discount (if any)
            discount = await _context.Discounts
                .Where(d => d.BusinessId == businessId 
                    && d.IsActive
                    && (d.ValidFrom == null || d.ValidFrom <= now)
                    && (d.ValidTo == null || d.ValidTo >= now))
                .OrderByDescending(d => d.CreatedAt)
                .FirstOrDefaultAsync();
        }

        if (discount == null)
        {
            return 0;
        }

        return discount.Type == DiscountType.Percentage
            ? subtotal * (discount.Value / 100)
            : Math.Min(discount.Value, subtotal); // Fixed amount, but not more than subtotal
    }

    /// <summary>
    /// Calculate final total: SubTotal - Discount + Tax
    /// </summary>
    public decimal CalculateTotal(decimal subtotal, decimal discount, decimal tax)
    {
        return subtotal - discount + tax;
    }

    /// <summary>
    /// Calculate all order totals (subtotal, tax, discount, total)
    /// </summary>
    public async Task<OrderTotals> CalculateOrderTotalsAsync(Order order, int? discountId = null)
    {
        var subtotal = CalculateSubtotal(order);
        var tax = await CalculateTaxAsync(subtotal, order.BusinessId);
        var discount = await ApplyDiscountAsync(subtotal, order.BusinessId, discountId);
        var total = CalculateTotal(subtotal, discount, tax);

        return new OrderTotals
        {
            SubTotal = subtotal,
            Tax = tax,
            Discount = discount,
            Total = total
        };
    }

    /// <summary>
    /// Get all active discounts for a business
    /// </summary>
    public async Task<List<Discount>> GetActiveDiscountsAsync(int businessId)
    {
        var now = DateTime.UtcNow;
        return await _context.Discounts
            .Where(d => d.BusinessId == businessId 
                && d.IsActive
                && (d.ValidFrom == null || d.ValidFrom <= now)
                && (d.ValidTo == null || d.ValidTo >= now))
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Get all active tax rates for a business
    /// </summary>
    public async Task<List<Tax>> GetActiveTaxesAsync(int businessId)
    {
        var now = DateTime.UtcNow;
        return await _context.Taxes
            .Where(t => t.BusinessId == businessId 
                && t.IsActive 
                && t.EffectiveFrom <= now
                && (t.EffectiveTo == null || t.EffectiveTo >= now))
            .ToListAsync();
    }
}

/// <summary>
/// Result of order total calculations
/// </summary>
public class OrderTotals
{
    public decimal SubTotal { get; set; }
    public decimal Tax { get; set; }
    public decimal Discount { get; set; }
    public decimal Total { get; set; }
}
