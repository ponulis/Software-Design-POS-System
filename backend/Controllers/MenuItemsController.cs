using backend.DTOs;
using backend.Extensions;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/menu-items")]
[Authorize]
public class MenuItemsController : ControllerBase
{
    private readonly ProductService _productService;
    private readonly ILogger<MenuItemsController> _logger;

    public MenuItemsController(ProductService productService, ILogger<MenuItemsController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    /// <summary>
    /// Get all menu items (products) for the authenticated user's business
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<ProductResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllMenuItems([FromQuery] bool? availableOnly = null)
    {
        try
        {
            var businessIdNullable = User.GetBusinessId();
            if (!businessIdNullable.HasValue)
            {
                throw new UnauthorizedAccessException("Business ID not found in token");
            }

            var businessId = businessIdNullable.Value;
            var products = await _productService.GetAllProductsAsync(businessId, availableOnly);
            return Ok(products);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving menu items: {Message}", ex.Message);
            return StatusCode(500, new { message = $"An error occurred while retrieving menu items: {ex.Message}" });
        }
    }

    /// <summary>
    /// Get menu item (product) by ID
    /// </summary>
    [HttpGet("{menuItemId}")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMenuItemById(int menuItemId)
    {
        try
        {
            var businessIdNullable = User.GetBusinessId();
            if (!businessIdNullable.HasValue)
            {
                throw new UnauthorizedAccessException("Business ID not found in token");
            }

            var businessId = businessIdNullable.Value;
            var product = await _productService.GetProductByIdAsync(menuItemId, businessId);

            if (product == null)
            {
                return NotFound(new { message = "Menu item not found" });
            }

            return Ok(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving menu item");
            return StatusCode(500, new { message = "An error occurred while retrieving the menu item" });
        }
    }

    /// <summary>
    /// Create a new menu item (product)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Manager,Admin")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateMenuItem([FromBody] CreateProductRequest request)
    {
        try
        {
            var businessIdNullable = User.GetBusinessId();
            if (!businessIdNullable.HasValue)
            {
                throw new UnauthorizedAccessException("Business ID not found in token");
            }

            var businessId = businessIdNullable.Value;

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest(new { message = "Name is required" });
            }

            if (request.Price < 0)
            {
                return BadRequest(new { message = "Price must be non-negative" });
            }

            var product = await _productService.CreateProductAsync(request, businessId);
            return CreatedAtAction(nameof(GetMenuItemById), new { menuItemId = product.Id }, product);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Validation error creating menu item: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating menu item: {Message}", ex.Message);
            return StatusCode(500, new { message = $"An error occurred while creating the menu item: {ex.Message}" });
        }
    }

    /// <summary>
    /// Update a menu item (product)
    /// </summary>
    [HttpPatch("{menuItemId}")]
    [Authorize(Roles = "Manager,Admin")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateMenuItem(int menuItemId, [FromBody] UpdateProductRequest request)
    {
        try
        {
            var businessIdNullable = User.GetBusinessId();
            if (!businessIdNullable.HasValue)
            {
                throw new UnauthorizedAccessException("Business ID not found in token");
            }

            var businessId = businessIdNullable.Value;

            if (request.Price.HasValue && request.Price.Value < 0)
            {
                return BadRequest(new { message = "Price must be non-negative" });
            }

            var product = await _productService.UpdateProductAsync(menuItemId, request, businessId);

            if (product == null)
            {
                return NotFound(new { message = "Menu item not found" });
            }

            return Ok(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating menu item");
            return StatusCode(500, new { message = "An error occurred while updating the menu item" });
        }
    }

    /// <summary>
    /// Delete a menu item (product)
    /// </summary>
    [HttpDelete("{menuItemId}")]
    [Authorize(Roles = "Manager,Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteMenuItem(int menuItemId)
    {
        try
        {
            var businessIdNullable = User.GetBusinessId();
            if (!businessIdNullable.HasValue)
            {
                throw new UnauthorizedAccessException("Business ID not found in token");
            }

            var businessId = businessIdNullable.Value;
            var deleted = await _productService.DeleteProductAsync(menuItemId, businessId);

            if (!deleted)
            {
                return NotFound(new { message = "Menu item not found" });
            }

            return Ok(new { message = "Menu item deleted successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting menu item");
            return StatusCode(500, new { message = "An error occurred while deleting the menu item" });
        }
    }
}
