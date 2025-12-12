using backend.DTOs;
using backend.Extensions;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/order-items")]
[Authorize]
public class OrderItemsController : ControllerBase
{
    private readonly OrderItemService _orderItemService;
    private readonly ILogger<OrderItemsController> _logger;

    public OrderItemsController(OrderItemService orderItemService, ILogger<OrderItemsController> logger)
    {
        _orderItemService = orderItemService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new order item
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(OrderItemResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateOrderItem([FromBody] CreateOrderItemRequest request)
    {
        try
        {
            var businessId = User.GetBusinessId() ?? throw new UnauthorizedAccessException("Business ID not found in token");
            var orderItem = await _orderItemService.CreateOrderItemAsync(request, businessId);
            if (orderItem == null)
            {
                return BadRequest(new { message = "Failed to create order item" });
            }
            return CreatedAtAction(nameof(GetOrderItemById), new { orderItemId = orderItem.Id }, orderItem);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order item");
            return StatusCode(500, new { message = "An error occurred while creating the order item" });
        }
    }

    /// <summary>
    /// Get all order items (optionally filtered by orderId)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<OrderItemResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllOrderItems([FromQuery] int? orderId)
    {
        try
        {
            var businessId = User.GetBusinessId() ?? throw new UnauthorizedAccessException("Business ID not found in token");
            var orderItems = await _orderItemService.GetAllOrderItemsAsync(orderId, businessId);
            return Ok(orderItems);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving order items");
            return StatusCode(500, new { message = "An error occurred while retrieving order items" });
        }
    }

    /// <summary>
    /// Get order item by ID
    /// </summary>
    [HttpGet("{orderItemId}")]
    [ProducesResponseType(typeof(OrderItemResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrderItemById(int orderItemId)
    {
        try
        {
            var businessId = User.GetBusinessId() ?? throw new UnauthorizedAccessException("Business ID not found in token");
            var orderItem = await _orderItemService.GetOrderItemByIdAsync(orderItemId, businessId);

            if (orderItem == null)
            {
                return NotFound(new { message = "Order item not found" });
            }

            return Ok(orderItem);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving order item");
            return StatusCode(500, new { message = "An error occurred while retrieving the order item" });
        }
    }

    /// <summary>
    /// Update an order item
    /// </summary>
    [HttpPatch("{orderItemId}")]
    [ProducesResponseType(typeof(OrderItemResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateOrderItem(int orderItemId, [FromBody] UpdateOrderItemRequest request)
    {
        try
        {
            var businessId = User.GetBusinessId() ?? throw new UnauthorizedAccessException("Business ID not found in token");
            var orderItem = await _orderItemService.UpdateOrderItemAsync(orderItemId, request, businessId);

            if (orderItem == null)
            {
                return NotFound(new { message = "Order item not found" });
            }

            return Ok(orderItem);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating order item");
            return StatusCode(500, new { message = "An error occurred while updating the order item" });
        }
    }

    /// <summary>
    /// Delete an order item
    /// </summary>
    [HttpDelete("{orderItemId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteOrderItem(int orderItemId)
    {
        try
        {
            var businessId = User.GetBusinessId() ?? throw new UnauthorizedAccessException("Business ID not found in token");
            var deleted = await _orderItemService.DeleteOrderItemAsync(orderItemId, businessId);

            if (!deleted)
            {
                return NotFound(new { message = "Order item not found" });
            }

            return Ok(new { message = "Order item deleted successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting order item");
            return StatusCode(500, new { message = "An error occurred while deleting the order item" });
        }
    }
}
