using backend.Data;
using backend.DTOs;
using backend.Extensions;
using backend.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace backend.Services;

public class OrderService
{
    private readonly ApplicationDbContext _context;
    private readonly PricingService _pricingService;
    private readonly OrderValidationService _validationService;

    public OrderService(
        ApplicationDbContext context, 
        PricingService pricingService,
        OrderValidationService validationService)
    {
        _context = context;
        _pricingService = pricingService;
        _validationService = validationService;
    }

    public async Task<OrderResponse?> CreateOrderAsync(CreateOrderRequest request, int businessId, int userId)
    {
        // Validate products availability
        var productIds = request.Items.Select(i => i.MenuId).ToList();
        var products = await _context.Products
            .Where(p => productIds.Contains(p.Id) && p.BusinessId == businessId)
            .ToListAsync();

        if (products.Count != productIds.Count)
        {
            throw new InvalidOperationException("One or more products not found or don't belong to your business");
        }

        var unavailableProducts = products.Where(p => !p.Available).ToList();
        if (unavailableProducts.Any())
        {
            throw new InvalidOperationException($"Products not available: {string.Join(", ", unavailableProducts.Select(p => p.Name))}");
        }

        // Create order
        var order = new Order
        {
            BusinessId = businessId,
            SpotId = request.SpotId,
            CreatedBy = userId,
            Status = string.IsNullOrEmpty(request.Status) 
                ? OrderStatus.Pending 
                : Enum.Parse<OrderStatus>(request.Status, true),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Create order items
        foreach (var itemRequest in request.Items)
        {
            var product = products.First(p => p.Id == itemRequest.MenuId);
            var orderItem = new OrderItem
            {
                // OrderId will be automatically set by EF Core when order is saved via navigation property
                MenuId = itemRequest.MenuId,
                Quantity = itemRequest.Quantity,
                Price = itemRequest.Price ?? product.Price,
                Notes = itemRequest.Notes
            };
            order.Items.Add(orderItem);
        }

        // Calculate totals using PricingService
        var totals = await _pricingService.CalculateOrderTotalsAsync(order, null);
        order.SubTotal = totals.SubTotal;
        order.Tax = totals.Tax;
        order.Discount = request.Discount ?? totals.Discount; // Use provided discount or calculated
        // Total is calculated property: SubTotal - Discount + Tax

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        return MapToOrderResponse(order);
    }

    public async Task<PaginatedResponse<OrderResponse>> GetAllOrdersAsync(
        int businessId,
        string? status = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int? spotId = null,
        int page = 1,
        int pageSize = 50)
    {
        var query = _context.Orders
            .Where(o => o.BusinessId == businessId);

        // Filter by status
        if (!string.IsNullOrWhiteSpace(status))
        {
            if (Enum.TryParse<OrderStatus>(status, true, out var orderStatus))
            {
                query = query.Where(o => o.Status == orderStatus);
            }
        }

        // Filter by date range
        if (startDate.HasValue)
        {
            query = query.Where(o => o.CreatedAt >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(o => o.CreatedAt <= endDate.Value);
        }

        // Filter by spot ID
        if (spotId.HasValue)
        {
            query = query.Where(o => o.SpotId == spotId.Value);
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply pagination
        var orders = await query
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var orderResponses = orders.Select(MapToOrderResponse).ToList();

        return new PaginatedResponse<OrderResponse>
        {
            Data = orderResponses,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<OrderResponse?> GetOrderByIdAsync(int orderId, int businessId)
    {
        var order = await _context.Orders
            .Where(o => o.Id == orderId && o.BusinessId == businessId)
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync();

        return order != null ? MapToOrderResponse(order) : null;
    }

    public async Task<OrderResponse?> UpdateOrderAsync(int orderId, UpdateOrderRequest request, int businessId)
    {
        var order = await _context.Orders
            .Where(o => o.Id == orderId && o.BusinessId == businessId)
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync();

        if (order == null)
        {
            return null;
        }

        // Validate order can be modified
        var validation = await _validationService.ValidateOrderForModificationAsync(orderId, businessId);
        if (!validation.IsValid)
        {
            throw new InvalidOperationException(validation.ErrorMessage ?? "Order cannot be modified");
        }

        // Update spot if provided
        if (request.SpotId.HasValue)
        {
            order.SpotId = request.SpotId.Value;
        }

        // Update status if provided (with validation)
        if (!string.IsNullOrEmpty(request.Status))
        {
            var newStatus = Enum.Parse<OrderStatus>(request.Status, true);
            var statusValidation = _validationService.ValidateStatusTransition(order.Status, newStatus);
            if (!statusValidation.IsValid)
            {
                throw new InvalidOperationException(statusValidation.ErrorMessage ?? "Invalid status transition");
            }
            order.Status = newStatus;
        }

        // Update items if provided
        if (request.Items != null && request.Items.Any())
        {
            // Remove existing items
            _context.OrderItems.RemoveRange(order.Items);

            // Validate new products
            var productIds = request.Items.Select(i => i.MenuId).ToList();
            var products = await _context.Products
                .Where(p => productIds.Contains(p.Id) && p.BusinessId == businessId)
                .ToListAsync();

            if (products.Count != productIds.Count)
            {
                throw new InvalidOperationException("One or more products not found or don't belong to your business");
            }

            var unavailableProducts = products.Where(p => !p.Available).ToList();
            if (unavailableProducts.Any())
            {
                throw new InvalidOperationException($"Products not available: {string.Join(", ", unavailableProducts.Select(p => p.Name))}");
            }

            // Add new items
            foreach (var itemRequest in request.Items)
            {
                var product = products.First(p => p.Id == itemRequest.MenuId);
                var orderItem = new OrderItem
                {
                    // OrderId will be automatically set by EF Core when order is saved via navigation property
                    MenuId = itemRequest.MenuId,
                    Quantity = itemRequest.Quantity,
                    Price = itemRequest.Price ?? product.Price,
                    Notes = itemRequest.Notes
                };
                order.Items.Add(orderItem);
            }
        }

        // Recalculate totals using PricingService
        var totals = await _pricingService.CalculateOrderTotalsAsync(order, null);
        
        // Use provided values or calculated values
        order.SubTotal = request.SubTotal ?? totals.SubTotal;
        order.Discount = request.Discount ?? totals.Discount;
        order.Tax = request.Tax ?? totals.Tax;
        // Total is calculated property: SubTotal - Discount + Tax

        order.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapToOrderResponse(order);
    }

    /// <summary>
    /// Place an order (transition from Draft to Placed)
    /// Based on Section 4 of ORDER_MANAGEMENT_PLAN.md
    /// </summary>
    public async Task<OrderResponse?> PlaceOrderAsync(int orderId, int businessId)
    {
        var order = await _context.Orders
            .Where(o => o.Id == orderId && o.BusinessId == businessId)
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync();

        if (order == null)
        {
            return null;
        }

        // Validate order can be placed
        var validation = await _validationService.ValidateOrderForPlacementAsync(orderId, businessId);
        if (!validation.IsValid)
        {
            throw new InvalidOperationException(validation.ErrorMessage ?? "Order validation failed");
        }

        // Validate status transition
        var statusValidation = _validationService.ValidateStatusTransition(order.Status, OrderStatus.Placed);
        if (!statusValidation.IsValid)
        {
            throw new InvalidOperationException(statusValidation.ErrorMessage ?? "Invalid status transition");
        }

        // Transition to Placed status
        order.Status = OrderStatus.Placed;
        order.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapToOrderResponse(order);
    }

    /// <summary>
    /// Cancel an order (transition to Cancelled status)
    /// Based on Section 8 of ORDER_MANAGEMENT_PLAN.md
    /// </summary>
    public async Task<OrderResponse?> CancelOrderAsync(int orderId, int businessId, string? reason = null)
    {
        var order = await _context.Orders
            .Where(o => o.Id == orderId && o.BusinessId == businessId)
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .Include(o => o.Payments)
            .FirstOrDefaultAsync();

        if (order == null)
        {
            return null;
        }

        // Validate order can be cancelled
        var validation = await _validationService.ValidateOrderForCancellationAsync(orderId, businessId);
        if (!validation.IsValid)
        {
            throw new InvalidOperationException(validation.ErrorMessage ?? "Order cannot be cancelled");
        }

        // Validate status transition
        var statusValidation = _validationService.ValidateStatusTransition(order.Status, OrderStatus.Cancelled);
        if (!statusValidation.IsValid)
        {
            throw new InvalidOperationException(statusValidation.ErrorMessage ?? "Invalid status transition");
        }

        // Transition to Cancelled status
        order.Status = OrderStatus.Cancelled;
        order.UpdatedAt = DateTime.UtcNow;

        // TODO: If order has payments, process refunds (if applicable)
        // TODO: Reverse gift card deductions (if applicable)
        // TODO: Release reserved inventory
        // TODO: Release reserved time slots (for appointments)

        await _context.SaveChangesAsync();

        return MapToOrderResponse(order);
    }

    public async Task<bool> DeleteOrderAsync(int orderId, int businessId)
    {
        var order = await _context.Orders
            .Where(o => o.Id == orderId && o.BusinessId == businessId)
            .FirstOrDefaultAsync();

        if (order == null)
        {
            return false;
        }

        // Cannot delete paid orders - should be cancelled instead
        if (order.Status == OrderStatus.Paid)
        {
            throw new InvalidOperationException("Cannot delete paid orders. Cancel the order instead.");
        }

        // Delete order items first
        var orderItems = await _context.OrderItems
            .Where(oi => oi.OrderId == orderId)
            .ToListAsync();
        _context.OrderItems.RemoveRange(orderItems);

        _context.Orders.Remove(order);
        await _context.SaveChangesAsync();

        return true;
    }


    public async Task<ReceiptResponse?> GetReceiptAsync(int orderId, int businessId)
    {
        var order = await _context.Orders
            .Where(o => o.Id == orderId && o.BusinessId == businessId)
            .Include(o => o.Business)
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .Include(o => o.Payments)
            .Include(o => o.Creator)
            .FirstOrDefaultAsync();

        if (order == null)
        {
            return null;
        }

        return MapToReceiptResponse(order);
    }

    private OrderResponse MapToOrderResponse(Order order)
    {
        return new OrderResponse
        {
            Id = order.Id,
            SpotId = order.SpotId,
            CreatedBy = order.CreatedBy,
            Items = order.Items.Select(i => new OrderItemResponse
            {
                Id = i.Id,
                MenuId = i.MenuId,
                Quantity = i.Quantity,
                Price = i.Price,
                Notes = i.Notes
            }).ToList(),
            SubTotal = order.SubTotal,
            Discount = order.Discount,
            Tax = order.Tax,
            Total = order.Total,
            Status = order.Status.ToString(),
            UpdatedAt = order.UpdatedAt,
            CreatedAt = order.CreatedAt
        };
    }

    private ReceiptResponse MapToReceiptResponse(Order order)
    {
        var totalPaid = order.Payments?.Sum(p => p.Amount) ?? 0;
        var remainingBalance = order.Total - totalPaid;

        return new ReceiptResponse
        {
            OrderId = order.Id,
            OrderNumber = $"ORD-{order.Id:D6}",
            OrderDate = order.CreatedAt,
            Status = order.Status.ToString(),
            
            // Business Information
            BusinessName = order.Business?.Name ?? string.Empty,
            BusinessDescription = order.Business?.Description,
            BusinessAddress = order.Business?.Address ?? string.Empty,
            BusinessPhone = order.Business?.PhoneNumber ?? string.Empty,
            BusinessEmail = order.Business?.ContactEmail ?? string.Empty,
            
            // Order Items
            Items = order.Items.Select(i => new ReceiptItemResponse
            {
                Name = i.Product?.Name ?? "Unknown Product",
                Quantity = i.Quantity,
                UnitPrice = i.Price,
                TotalPrice = i.Price * i.Quantity
            }).ToList(),
            
            // Totals
            SubTotal = order.SubTotal,
            Discount = order.Discount,
            Tax = order.Tax,
            Total = order.Total,
            
            // Payment Information
            Payments = order.Payments?.Select(p => new ReceiptPaymentResponse
            {
                PaymentId = p.Id,
                Method = p.Method.ToString(),
                Amount = p.Amount,
                PaidAt = p.PaidAt,
                TransactionId = p.TransactionId
            }).ToList() ?? new List<ReceiptPaymentResponse>(),
            TotalPaid = totalPaid,
            RemainingBalance = remainingBalance,
            
            // Employee Information
            CreatedByName = order.Creator?.Name
        };
    }
}
