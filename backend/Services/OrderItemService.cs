using backend.Data;
using backend.DTOs;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class OrderItemService
{
    private readonly ApplicationDbContext _context;
    private readonly PricingService _pricingService;

    public OrderItemService(ApplicationDbContext context, PricingService pricingService)
    {
        _context = context;
        _pricingService = pricingService;
    }

    public async Task<OrderItemResponse?> CreateOrderItemAsync(CreateOrderItemRequest request, int businessId)
    {
        if (!request.OrderId.HasValue)
        {
            throw new InvalidOperationException("OrderId is required");
        }

        // Verify order belongs to business
        var order = await _context.Orders
            .Where(o => o.Id == request.OrderId.Value && o.BusinessId == businessId)
            .FirstOrDefaultAsync();

        if (order == null)
        {
            throw new InvalidOperationException("Order not found or doesn't belong to your business");
        }

        // Cannot add items to paid or cancelled orders
        if (order.Status == OrderStatus.Paid || order.Status == OrderStatus.Cancelled)
        {
            throw new InvalidOperationException("Cannot add items to paid or cancelled orders");
        }

        // Validate product
        var product = await _context.Products
            .Where(p => p.Id == request.MenuId && p.BusinessId == businessId)
            .FirstOrDefaultAsync();

        if (product == null)
        {
            throw new InvalidOperationException("Product not found or doesn't belong to your business");
        }

        if (!product.Available)
        {
            throw new InvalidOperationException("Product is not available");
        }

        var orderItem = new OrderItem
        {
            OrderId = request.OrderId.Value,
            MenuId = request.MenuId,
            Quantity = request.Quantity,
            Price = request.Price ?? product.Price,
            Notes = request.Notes
        };

        _context.OrderItems.Add(orderItem);
        await _context.SaveChangesAsync();

        // Recalculate order totals
        await RecalculateOrderTotalsAsync(order.Id);

        return MapToOrderItemResponse(orderItem);
    }

    public async Task<List<OrderItemResponse>> GetAllOrderItemsAsync(int? orderId, int businessId)
    {
        var query = _context.OrderItems
            .Include(oi => oi.Order)
            .Where(oi => oi.Order.BusinessId == businessId);

        if (orderId.HasValue)
        {
            query = query.Where(oi => oi.OrderId == orderId.Value);
        }

        var orderItems = await query.ToListAsync();
        return orderItems.Select(MapToOrderItemResponse).ToList();
    }

    public async Task<OrderItemResponse?> GetOrderItemByIdAsync(int orderItemId, int businessId)
    {
        var orderItem = await _context.OrderItems
            .Include(oi => oi.Order)
            .Where(oi => oi.Id == orderItemId && oi.Order.BusinessId == businessId)
            .FirstOrDefaultAsync();

        return orderItem != null ? MapToOrderItemResponse(orderItem) : null;
    }

    public async Task<OrderItemResponse?> UpdateOrderItemAsync(int orderItemId, UpdateOrderItemRequest request, int businessId)
    {
        var orderItem = await _context.OrderItems
            .Include(oi => oi.Order)
            .Where(oi => oi.Id == orderItemId && oi.Order.BusinessId == businessId)
            .FirstOrDefaultAsync();

        if (orderItem == null)
        {
            return null;
        }

        // Cannot update items in paid or cancelled orders
        if (orderItem.Order.Status == OrderStatus.Paid || orderItem.Order.Status == OrderStatus.Cancelled)
        {
            throw new InvalidOperationException("Cannot update items in paid or cancelled orders");
        }

        // Update fields if provided
        if (request.MenuId.HasValue)
        {
            // Validate new product
            var product = await _context.Products
                .Where(p => p.Id == request.MenuId.Value && p.BusinessId == businessId)
                .FirstOrDefaultAsync();

            if (product == null)
            {
                throw new InvalidOperationException("Product not found or doesn't belong to your business");
            }

            if (!product.Available)
            {
                throw new InvalidOperationException("Product is not available");
            }

            orderItem.MenuId = request.MenuId.Value;
            orderItem.Price = request.Price ?? product.Price;
        }

        if (request.Quantity.HasValue)
        {
            orderItem.Quantity = request.Quantity.Value;
        }

        if (request.Price.HasValue && !request.MenuId.HasValue)
        {
            orderItem.Price = request.Price.Value;
        }

        if (request.Notes != null)
        {
            orderItem.Notes = request.Notes;
        }

        await _context.SaveChangesAsync();

        // Recalculate order totals
        await RecalculateOrderTotalsAsync(orderItem.OrderId);

        return MapToOrderItemResponse(orderItem);
    }

    public async Task<bool> DeleteOrderItemAsync(int orderItemId, int businessId)
    {
        var orderItem = await _context.OrderItems
            .Include(oi => oi.Order)
            .Where(oi => oi.Id == orderItemId && oi.Order.BusinessId == businessId)
            .FirstOrDefaultAsync();

        if (orderItem == null)
        {
            return false;
        }

        // Cannot delete items from paid or cancelled orders
        if (orderItem.Order.Status == OrderStatus.Paid || orderItem.Order.Status == OrderStatus.Cancelled)
        {
            throw new InvalidOperationException("Cannot delete items from paid or cancelled orders");
        }

        var orderId = orderItem.OrderId;
        _context.OrderItems.Remove(orderItem);
        await _context.SaveChangesAsync();

        // Recalculate order totals
        await RecalculateOrderTotalsAsync(orderId);

        return true;
    }

    private async Task RecalculateOrderTotalsAsync(int orderId)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order != null)
        {
            // Recalculate all totals using PricingService
            var totals = await _pricingService.CalculateOrderTotalsAsync(order, null);
            order.SubTotal = totals.SubTotal;
            order.Tax = totals.Tax;
            order.Discount = totals.Discount;
            order.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    private OrderItemResponse MapToOrderItemResponse(OrderItem orderItem)
    {
        return new OrderItemResponse
        {
            Id = orderItem.Id,
            MenuId = orderItem.MenuId,
            Quantity = orderItem.Quantity,
            Price = orderItem.Price,
            Notes = orderItem.Notes
        };
    }
}
