using backend.Data;
using backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace backend.Services;

/// <summary>
/// Service for managing inventory operations
/// </summary>
public class InventoryService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<InventoryService> _logger;

    public InventoryService(ApplicationDbContext context, ILogger<InventoryService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Deduct inventory for order items when order is paid
    /// </summary>
    public async Task DeductInventoryForOrderAsync(Order order)
    {
        if (order.Items == null || !order.Items.Any())
        {
            _logger.LogWarning("Order {OrderId} has no items to deduct inventory for", order.Id);
            return;
        }

        var errors = new List<string>();

        foreach (var orderItem in order.Items)
        {
            try
            {
                await DeductInventoryForOrderItemAsync(orderItem.ProductId, orderItem.Quantity, order.BusinessId);
            }
            catch (InvalidOperationException ex)
            {
                errors.Add($"Product ID {orderItem.ProductId}: {ex.Message}");
                _logger.LogError(ex, "Failed to deduct inventory for ProductId={ProductId}, Quantity={Quantity} in OrderId={OrderId}",
                    orderItem.ProductId, orderItem.Quantity, order.Id);
            }
        }

        if (errors.Any())
        {
            throw new InvalidOperationException($"Failed to deduct inventory for some items:\n{string.Join("\n", errors)}");
        }

        _logger.LogInformation("Successfully deducted inventory for OrderId={OrderId}", order.Id);
    }

    /// <summary>
    /// Deduct inventory for a specific product and quantity
    /// </summary>
    private async Task DeductInventoryForOrderItemAsync(int productId, int quantity, int businessId)
    {
        // Get all inventory items for this product
        var inventoryItems = await _context.InventoryItems
            .Where(ii => ii.ProductId == productId && ii.Product.BusinessId == businessId)
            .OrderBy(ii => ii.Id) // Consistent ordering
            .ToListAsync();

        if (!inventoryItems.Any())
        {
            // No inventory tracking for this product - skip deduction
            _logger.LogInformation("No inventory items found for ProductId={ProductId}, skipping deduction", productId);
            return;
        }

        // For products without modifications, use the inventory item with empty ModificationValuesJson
        // For products with modifications, we'll deduct from available inventory items
        var remainingQuantity = quantity;

        // First, try to deduct from inventory items without modifications (empty JSON)
        var simpleInventoryItems = inventoryItems
            .Where(ii => string.IsNullOrWhiteSpace(ii.ModificationValuesJson) || 
                        ii.ModificationValuesJson == "{}")
            .OrderBy(ii => ii.Id)
            .ToList();

        foreach (var inventoryItem in simpleInventoryItems)
        {
            if (remainingQuantity <= 0)
                break;

            if (inventoryItem.Quantity >= remainingQuantity)
            {
                inventoryItem.Quantity -= remainingQuantity;
                inventoryItem.UpdatedAt = DateTime.UtcNow;
                remainingQuantity = 0;
            }
            else
            {
                remainingQuantity -= inventoryItem.Quantity;
                inventoryItem.Quantity = 0;
                inventoryItem.UpdatedAt = DateTime.UtcNow;
            }
        }

        // If still need to deduct more, use other inventory items (with modifications)
        if (remainingQuantity > 0)
        {
            var modifiedInventoryItems = inventoryItems
                .Where(ii => !string.IsNullOrWhiteSpace(ii.ModificationValuesJson) && 
                            ii.ModificationValuesJson != "{}")
                .OrderBy(ii => ii.Id)
                .ToList();

            foreach (var inventoryItem in modifiedInventoryItems)
            {
                if (remainingQuantity <= 0)
                    break;

                if (inventoryItem.Quantity >= remainingQuantity)
                {
                    inventoryItem.Quantity -= remainingQuantity;
                    inventoryItem.UpdatedAt = DateTime.UtcNow;
                    remainingQuantity = 0;
                }
                else
                {
                    remainingQuantity -= inventoryItem.Quantity;
                    inventoryItem.Quantity = 0;
                    inventoryItem.UpdatedAt = DateTime.UtcNow;
                }
            }
        }

        if (remainingQuantity > 0)
        {
            throw new InvalidOperationException(
                $"Insufficient inventory for ProductId={productId}. Requested: {quantity}, Available: {quantity - remainingQuantity}");
        }

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Check if sufficient inventory exists for order items
    /// </summary>
    public async Task<bool> CheckInventoryAvailabilityAsync(Order order)
    {
        if (order.Items == null || !order.Items.Any())
        {
            return true;
        }

        foreach (var orderItem in order.Items)
        {
            var totalInventory = await _context.InventoryItems
                .Where(ii => ii.ProductId == orderItem.ProductId && ii.Product.BusinessId == order.BusinessId)
                .SumAsync(ii => ii.Quantity);

            if (totalInventory < orderItem.Quantity)
            {
                return false;
            }
        }

        return true;
    }
}
