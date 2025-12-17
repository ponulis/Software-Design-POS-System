using backend.Data;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

/// <summary>
/// Service for validating orders according to business rules defined in ORDER_MANAGEMENT_PLAN.md
/// </summary>
public class OrderValidationService
{
    private readonly ApplicationDbContext _context;

    public OrderValidationService(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Validates order before allowing payment processing
    /// Based on Section 3.1 of ORDER_MANAGEMENT_PLAN.md
    /// </summary>
    public async Task<OrderValidationResult> ValidateOrderForPaymentAsync(int orderId, int businessId)
    {
        var order = await _context.Orders
            .Where(o => o.Id == orderId && o.BusinessId == businessId)
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync();

        if (order == null)
        {
            return OrderValidationResult.Failure("Order not found or doesn't belong to your business");
        }

        // 1. Order Status Validation
        if (order.Status == OrderStatus.Paid)
        {
            return OrderValidationResult.Failure("Order is already paid. Cannot process additional payments.");
        }

        if (order.Status == OrderStatus.Cancelled)
        {
            return OrderValidationResult.Failure("Order has been cancelled. Cannot process payment.");
        }

        if (order.Status != OrderStatus.Draft && order.Status != OrderStatus.Placed)
        {
            return OrderValidationResult.Failure($"Invalid order status: {order.Status}. Order must be Draft or Placed to process payment.");
        }

        // 2. Order Content Validation
        if (!order.Items.Any())
        {
            return OrderValidationResult.Failure("Order has no items. Cannot process payment.");
        }

        if (order.Total <= 0)
        {
            return OrderValidationResult.Failure($"Order total is invalid ({order.Total:C}). Cannot process payment.");
        }

        // 3. Stock Availability Validation
        var unavailableProducts = order.Items
            .Where(item => item.Product != null && !item.Product.Available)
            .Select(item => item.Product!.Name)
            .ToList();

        if (unavailableProducts.Any())
        {
            return OrderValidationResult.Failure(
                $"Products not available: {string.Join(", ", unavailableProducts)}. Cannot process payment.");
        }

        // 4. Validate all order items reference valid products
        var invalidItems = order.Items
            .Where(item => item.Product == null)
            .ToList();

        if (invalidItems.Any())
        {
            return OrderValidationResult.Failure(
                $"Order contains invalid items. {invalidItems.Count} item(s) reference non-existent products.");
        }

        // 5. Validate quantities are positive
        var invalidQuantities = order.Items
            .Where(item => item.Quantity <= 0)
            .ToList();

        if (invalidQuantities.Any())
        {
            return OrderValidationResult.Failure(
                "Order contains items with invalid quantities. All quantities must be greater than zero.");
        }

        return OrderValidationResult.Success();
    }

    /// <summary>
    /// Validates order can be placed (moved from Draft to Placed)
    /// </summary>
    public async Task<OrderValidationResult> ValidateOrderForPlacementAsync(int orderId, int businessId)
    {
        var order = await _context.Orders
            .Where(o => o.Id == orderId && o.BusinessId == businessId)
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync();

        if (order == null)
        {
            return OrderValidationResult.Failure("Order not found or doesn't belong to your business");
        }

        if (order.Status != OrderStatus.Draft)
        {
            return OrderValidationResult.Failure($"Order status is {order.Status}. Can only place orders with Draft status.");
        }

        // Validate order has items
        if (!order.Items.Any())
        {
            return OrderValidationResult.Failure("Cannot place order without items.");
        }

        // Validate order total is valid
        if (order.Total <= 0)
        {
            return OrderValidationResult.Failure($"Order total is invalid ({order.Total:C}). Cannot place order.");
        }

        // Validate all products are available
        var unavailableProducts = order.Items
            .Where(item => item.Product != null && !item.Product.Available)
            .Select(item => item.Product!.Name)
            .ToList();

        if (unavailableProducts.Any())
        {
            return OrderValidationResult.Failure(
                $"Cannot place order. Products not available: {string.Join(", ", unavailableProducts)}");
        }

        return OrderValidationResult.Success();
    }

    /// <summary>
    /// Validates order can be cancelled
    /// Based on Section 8.1 of ORDER_MANAGEMENT_PLAN.md
    /// </summary>
    public async Task<OrderValidationResult> ValidateOrderForCancellationAsync(int orderId, int businessId)
    {
        var order = await _context.Orders
            .Where(o => o.Id == orderId && o.BusinessId == businessId)
            .Include(o => o.Payments)
            .FirstOrDefaultAsync();

        if (order == null)
        {
            return OrderValidationResult.Failure("Order not found or doesn't belong to your business");
        }

        // Cannot cancel paid orders (must use refund instead)
        if (order.Status == OrderStatus.Paid)
        {
            return OrderValidationResult.Failure(
                "Cannot cancel paid orders. Process a refund instead if needed.");
        }

        // Cannot cancel already cancelled orders
        if (order.Status == OrderStatus.Cancelled)
        {
            return OrderValidationResult.Failure("Order is already cancelled.");
        }

        // Can cancel Draft or Placed orders
        if (order.Status != OrderStatus.Draft && order.Status != OrderStatus.Placed)
        {
            return OrderValidationResult.Failure($"Invalid order status: {order.Status}. Cannot cancel order.");
        }

        return OrderValidationResult.Success();
    }

    /// <summary>
    /// Validates order can be modified (items added/removed/changed)
    /// </summary>
    public async Task<OrderValidationResult> ValidateOrderForModificationAsync(int orderId, int businessId)
    {
        var order = await _context.Orders
            .Where(o => o.Id == orderId && o.BusinessId == businessId)
            .FirstOrDefaultAsync();

        if (order == null)
        {
            return OrderValidationResult.Failure("Order not found or doesn't belong to your business");
        }

        // Cannot modify paid or cancelled orders
        if (order.Status == OrderStatus.Paid)
        {
            return OrderValidationResult.Failure("Cannot modify paid orders.");
        }

        if (order.Status == OrderStatus.Cancelled)
        {
            return OrderValidationResult.Failure("Cannot modify cancelled orders.");
        }

        return OrderValidationResult.Success();
    }

    /// <summary>
    /// Validates status transition is allowed
    /// Based on Section 1.2 of ORDER_MANAGEMENT_PLAN.md
    /// </summary>
    public OrderValidationResult ValidateStatusTransition(OrderStatus currentStatus, OrderStatus newStatus)
    {
        // Same status is always valid
        if (currentStatus == newStatus)
        {
            return OrderValidationResult.Success();
        }

        // Valid transitions
        var validTransitions = new Dictionary<OrderStatus, List<OrderStatus>>
        {
            { OrderStatus.Draft, new List<OrderStatus> { OrderStatus.Placed, OrderStatus.Cancelled } },
            { OrderStatus.Placed, new List<OrderStatus> { OrderStatus.Paid, OrderStatus.Cancelled } },
            { OrderStatus.Paid, new List<OrderStatus>() }, // Paid is terminal (cannot transition from Paid)
            { OrderStatus.Cancelled, new List<OrderStatus>() } // Cancelled is terminal
        };

        if (!validTransitions.ContainsKey(currentStatus))
        {
            return OrderValidationResult.Failure($"Unknown current status: {currentStatus}");
        }

        var allowedTransitions = validTransitions[currentStatus];
        if (!allowedTransitions.Contains(newStatus))
        {
            return OrderValidationResult.Failure(
                $"Invalid status transition: {currentStatus} → {newStatus}. " +
                $"Allowed transitions from {currentStatus}: {string.Join(", ", allowedTransitions)}");
        }

        return OrderValidationResult.Success();
    }
}

/// <summary>
/// Result of order validation
/// </summary>
public class OrderValidationResult
{
    public bool IsValid { get; private set; }
    public string? ErrorMessage { get; private set; }

    private OrderValidationResult(bool isValid, string? errorMessage = null)
    {
        IsValid = isValid;
        ErrorMessage = errorMessage;
    }

    public static OrderValidationResult Success() => new OrderValidationResult(true);
    public static OrderValidationResult Failure(string message) => new OrderValidationResult(false, message);
}
