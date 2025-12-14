using backend.DTOs;
using backend.Extensions;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace backend.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly OrderService _orderService;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(OrderService orderService, ILogger<OrdersController> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    /// <summary>
    /// Get receipt for an order
    /// </summary>
    /// <param name="orderId">Order ID</param>
    /// <returns>Receipt details including business info, items, totals, and payments</returns>
    [HttpGet("{orderId}/receipt")]
    [ProducesResponseType(typeof(ReceiptResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetReceipt(int orderId)
    {
        try
        {
            var businessIdNullable = User.GetBusinessId();
            if (!businessIdNullable.HasValue)
            {
                throw new UnauthorizedAccessException("Business ID not found in token");
            }

            var businessId = businessIdNullable.Value;
            var receipt = await _orderService.GetReceiptAsync(orderId, businessId);

            if (receipt == null)
            {
                return NotFound(new { message = "Order not found" });
            }

            return Ok(receipt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving receipt");
            return StatusCode(500, new { message = "An error occurred while retrieving the receipt" });
        }
    }

    /// <summary>
    /// Create a new order
    /// </summary>
    /// <param name="request">Order details including items, spot ID, and optional discount</param>
    /// <returns>Created order with calculated totals</returns>
    /// <remarks>
    /// Sample request:
    /// 
    ///     POST /api/orders
    ///     {
    ///         "spotId": 1,
    ///         "items": [
    ///             {
    ///                 "menuId": 1,
    ///                 "quantity": 2,
    ///                 "price": 10.50
    ///             }
    ///         ],
    ///         "discount": 0
    ///     }
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        try
        {
            var businessIdNullable = User.GetBusinessId();
            var userIdNullable = User.GetUserId();
            
            if (!businessIdNullable.HasValue || !userIdNullable.HasValue)
            {
                throw new UnauthorizedAccessException("Business ID or User ID not found in token");
            }

            var businessId = businessIdNullable.Value;
            var userId = userIdNullable.Value;

            // Override CreatedBy with the authenticated user's ID
            request.CreatedBy = userId;

            var order = await _orderService.CreateOrderAsync(request, businessId, userId);
            if (order == null)
            {
                return BadRequest(new { message = "Failed to create order" });
            }
            return CreatedAtAction(nameof(GetOrderById), new { orderId = order.Id }, order);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order");
            return StatusCode(500, new { message = "An error occurred while creating the order" });
        }
    }

    /// <summary>
    /// Get all orders for the authenticated user's business
    /// </summary>
    /// <param name="status">Filter by order status (Draft, Placed, Paid, Cancelled)</param>
    /// <param name="startDate">Filter orders created on or after this date</param>
    /// <param name="endDate">Filter orders created on or before this date</param>
    /// <param name="spotId">Filter by spot ID</param>
    /// <returns>List of orders</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<OrderResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllOrders(
        [FromQuery] string? status = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int? spotId = null)
    {
        try
        {
            var businessId = User.GetBusinessId() ?? throw new UnauthorizedAccessException("Business ID not found in token");
            var orders = await _orderService.GetAllOrdersAsync(businessId, status, startDate, endDate, spotId);
            return Ok(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving orders");
            return StatusCode(500, new { message = "An error occurred while retrieving orders" });
        }
    }

    /// <summary>
    /// Get order by ID
    /// </summary>
    [HttpGet("{orderId}")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrderById(int orderId)
    {
        try
        {
            var businessId = User.GetBusinessId() ?? throw new UnauthorizedAccessException("Business ID not found in token");
            var order = await _orderService.GetOrderByIdAsync(orderId, businessId);

            if (order == null)
            {
                return NotFound(new { message = "Order not found" });
            }

            return Ok(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving order");
            return StatusCode(500, new { message = "An error occurred while retrieving the order" });
        }
    }

    /// <summary>
    /// Update an order
    /// </summary>
    [HttpPatch("{orderId}")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateOrder(int orderId, [FromBody] UpdateOrderRequest request)
    {
        try
        {
            var businessId = User.GetBusinessId() ?? throw new UnauthorizedAccessException("Business ID not found in token");
            var order = await _orderService.UpdateOrderAsync(orderId, request, businessId);

            if (order == null)
            {
                return NotFound(new { message = "Order not found" });
            }

            return Ok(order);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating order");
            return StatusCode(500, new { message = "An error occurred while updating the order" });
        }
    }

    /// <summary>
    /// Delete (cancel) an order
    /// </summary>
    [HttpDelete("{orderId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteOrder(int orderId)
    {
        try
        {
            var businessId = User.GetBusinessId() ?? throw new UnauthorizedAccessException("Business ID not found in token");
            var deleted = await _orderService.DeleteOrderAsync(orderId, businessId);

            if (!deleted)
            {
                return NotFound(new { message = "Order not found" });
            }

            return Ok(new { message = "Order deleted successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting order");
            return StatusCode(500, new { message = "An error occurred while deleting the order" });
        }
    }

    /// <summary>
    /// Process refund for an order
    /// </summary>
    /// <param name="orderId">Order ID</param>
    /// <param name="request">Refund request with optional reason and amount</param>
    /// <returns>Refund details including refunded payments</returns>
    /// <remarks>
    /// Sample request:
    /// 
    ///     POST /api/orders/{orderId}/refund
    ///     {
    ///         "reason": "Customer cancellation",
    ///         "amount": 50.00
    ///     }
    /// 
    /// Note: If amount is not provided, full refund will be processed.
    /// </remarks>
    [HttpPost("{orderId}/refund")]
    [Authorize(Roles = "Manager,Admin")]
    [ProducesResponseType(typeof(RefundResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ProcessRefund(int orderId, [FromBody] RefundRequest? request = null)
    {
        try
        {
            var businessIdNullable = User.GetBusinessId();
            if (!businessIdNullable.HasValue)
            {
                throw new UnauthorizedAccessException("Business ID not found in token");
            }

            var businessId = businessIdNullable.Value;
            var userIdNullable = User.GetUserId();
            if (!userIdNullable.HasValue)
            {
                throw new UnauthorizedAccessException("User ID not found in token");
            }
            var userId = userIdNullable.Value;

            request ??= new RefundRequest();

            var refundService = HttpContext.RequestServices.GetRequiredService<PaymentService>();
            var refund = await refundService.ProcessRefundAsync(orderId, request, businessId, userId);
            return Ok(refund);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing refund");
            return StatusCode(500, new { message = "An error occurred while processing the refund" });
        }
    }
}
