using backend.DTOs;
using backend.Extensions;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/product-modifications")]
[Authorize]
public class ProductModificationsController : ControllerBase
{
    private readonly ProductModificationService _modificationService;
    private readonly ILogger<ProductModificationsController> _logger;

    public ProductModificationsController(ProductModificationService modificationService, ILogger<ProductModificationsController> logger)
    {
        _modificationService = modificationService;
        _logger = logger;
    }

    /// <summary>
    /// Get all product modifications (attributes) for the authenticated user's business
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<ProductModificationResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllModifications()
    {
        try
        {
            var businessIdNullable = User.GetBusinessId();
            if (!businessIdNullable.HasValue)
            {
                throw new UnauthorizedAccessException("Business ID not found in token");
            }

            var businessId = businessIdNullable.Value;
            var modifications = await _modificationService.GetAllModificationsAsync(businessId);
            return Ok(modifications);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving product modifications: {Message}", ex.Message);
            return StatusCode(500, new { message = $"An error occurred while retrieving product modifications: {ex.Message}" });
        }
    }

    /// <summary>
    /// Get product modification by ID
    /// </summary>
    [HttpGet("{modificationId}")]
    [ProducesResponseType(typeof(ProductModificationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetModificationById(int modificationId)
    {
        try
        {
            var businessIdNullable = User.GetBusinessId();
            if (!businessIdNullable.HasValue)
            {
                throw new UnauthorizedAccessException("Business ID not found in token");
            }

            var businessId = businessIdNullable.Value;
            var modification = await _modificationService.GetModificationByIdAsync(modificationId, businessId);

            if (modification == null)
            {
                return NotFound(new { message = "Product modification not found" });
            }

            return Ok(modification);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving product modification");
            return StatusCode(500, new { message = "An error occurred while retrieving the product modification" });
        }
    }

    /// <summary>
    /// Create a new product modification (attribute)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Manager,Admin")]
    [ProducesResponseType(typeof(ProductModificationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateModification([FromBody] CreateProductModificationRequest request)
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

            var modification = await _modificationService.CreateModificationAsync(request, businessId);
            return CreatedAtAction(nameof(GetModificationById), new { modificationId = modification.Id }, modification);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product modification");
            return StatusCode(500, new { message = "An error occurred while creating the product modification" });
        }
    }

    /// <summary>
    /// Add a value to an existing modification
    /// </summary>
    [HttpPost("{modificationId}/values")]
    [Authorize(Roles = "Manager,Admin")]
    [ProducesResponseType(typeof(ProductModificationValueResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddValueToModification(int modificationId, [FromBody] AddModificationValueRequest request)
    {
        try
        {
            var businessIdNullable = User.GetBusinessId();
            if (!businessIdNullable.HasValue)
            {
                throw new UnauthorizedAccessException("Business ID not found in token");
            }

            var businessId = businessIdNullable.Value;

            if (string.IsNullOrWhiteSpace(request.Value))
            {
                return BadRequest(new { message = "Value is required" });
            }

            var value = await _modificationService.AddValueToModificationAsync(modificationId, request.Value, businessId);
            
            var response = new ProductModificationValueResponse
            {
                Id = value.Id,
                Value = value.Value,
                CreatedAt = value.CreatedAt
            };

            return CreatedAtAction(nameof(GetModificationById), new { modificationId = modificationId }, response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding value to modification");
            return StatusCode(500, new { message = "An error occurred while adding value to modification" });
        }
    }

    /// <summary>
    /// Delete a product modification
    /// </summary>
    [HttpDelete("{modificationId}")]
    [Authorize(Roles = "Manager,Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteModification(int modificationId)
    {
        try
        {
            var businessIdNullable = User.GetBusinessId();
            if (!businessIdNullable.HasValue)
            {
                throw new UnauthorizedAccessException("Business ID not found in token");
            }

            var businessId = businessIdNullable.Value;
            var deleted = await _modificationService.DeleteModificationAsync(modificationId, businessId);

            if (!deleted)
            {
                return NotFound(new { message = "Product modification not found" });
            }

            return Ok(new { message = "Product modification deleted successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product modification");
            return StatusCode(500, new { message = "An error occurred while deleting the product modification" });
        }
    }
}

public class AddModificationValueRequest
{
    public string Value { get; set; } = string.Empty;
}
