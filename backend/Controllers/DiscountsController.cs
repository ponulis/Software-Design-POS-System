using backend.DTOs;
using backend.Extensions;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/discounts")]
[Authorize]
public class DiscountsController : ControllerBase
{
    private readonly DiscountService _discountService;
    private readonly ILogger<DiscountsController> _logger;

    public DiscountsController(DiscountService discountService, ILogger<DiscountsController> logger)
    {
        _discountService = discountService;
        _logger = logger;
    }

    /// <summary>
    /// Get all discounts for the authenticated user's business
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<DiscountResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllDiscounts([FromQuery] bool? activeOnly = null)
    {
        try
        {
            var businessIdNullable = User.GetBusinessId();
            if (!businessIdNullable.HasValue)
            {
                throw new UnauthorizedAccessException("Business ID not found in token");
            }

            var businessId = businessIdNullable.Value;
            var discounts = await _discountService.GetAllDiscountsAsync(businessId, activeOnly);
            return Ok(discounts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving discounts");
            return StatusCode(500, new { message = "An error occurred while retrieving discounts" });
        }
    }

    /// <summary>
    /// Get discount by ID
    /// </summary>
    [HttpGet("{discountId}")]
    [ProducesResponseType(typeof(DiscountResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDiscountById(int discountId)
    {
        try
        {
            var businessIdNullable = User.GetBusinessId();
            if (!businessIdNullable.HasValue)
            {
                throw new UnauthorizedAccessException("Business ID not found in token");
            }

            var businessId = businessIdNullable.Value;
            var discount = await _discountService.GetDiscountByIdAsync(discountId, businessId);

            if (discount == null)
            {
                return NotFound(new { message = "Discount not found" });
            }

            return Ok(discount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving discount");
            return StatusCode(500, new { message = "An error occurred while retrieving the discount" });
        }
    }

    /// <summary>
    /// Create a new discount
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(DiscountResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateDiscount([FromBody] CreateDiscountRequest request)
    {
        try
        {
            var businessIdNullable = User.GetBusinessId();
            if (!businessIdNullable.HasValue)
            {
                throw new UnauthorizedAccessException("Business ID not found in token");
            }

            var businessId = businessIdNullable.Value;
            var discount = await _discountService.CreateDiscountAsync(request, businessId);
            return CreatedAtAction(nameof(GetDiscountById), new { discountId = discount.Id }, discount);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating discount");
            return StatusCode(500, new { message = "An error occurred while creating the discount" });
        }
    }

    /// <summary>
    /// Update discount (PATCH per API contract)
    /// </summary>
    [HttpPatch("{discountId}")]
    [ProducesResponseType(typeof(DiscountResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateDiscount(int discountId, [FromBody] UpdateDiscountRequest request)
    {
        try
        {
            var businessIdNullable = User.GetBusinessId();
            if (!businessIdNullable.HasValue)
            {
                throw new UnauthorizedAccessException("Business ID not found in token");
            }

            var businessId = businessIdNullable.Value;
            var discount = await _discountService.UpdateDiscountAsync(discountId, request, businessId);

            if (discount == null)
            {
                return NotFound(new { message = "Discount not found" });
            }

            return Ok(discount);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating discount");
            return StatusCode(500, new { message = "An error occurred while updating the discount" });
        }
    }

    /// <summary>
    /// Delete discount
    /// </summary>
    [HttpDelete("{discountId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteDiscount(int discountId)
    {
        try
        {
            var businessIdNullable = User.GetBusinessId();
            if (!businessIdNullable.HasValue)
            {
                throw new UnauthorizedAccessException("Business ID not found in token");
            }

            var businessId = businessIdNullable.Value;
            var deleted = await _discountService.DeleteDiscountAsync(discountId, businessId);

            if (!deleted)
            {
                return NotFound(new { message = "Discount not found" });
            }

            return Ok(new { message = "Discount deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting discount");
            return StatusCode(500, new { message = "An error occurred while deleting the discount" });
        }
    }
}
