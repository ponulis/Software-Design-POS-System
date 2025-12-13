using backend.DTOs;
using backend.Extensions;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/gift-cards")]
[Authorize]
public class GiftCardsController : ControllerBase
{
    private readonly GiftCardService _giftCardService;
    private readonly ILogger<GiftCardsController> _logger;

    public GiftCardsController(GiftCardService giftCardService, ILogger<GiftCardsController> logger)
    {
        _giftCardService = giftCardService;
        _logger = logger;
    }

    /// <summary>
    /// Validate gift card and get balance
    /// </summary>
    [HttpGet("{code}")]
    [ProducesResponseType(typeof(GiftCardResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetGiftCardByCode(string code)
    {
        try
        {
            var businessIdNullable = User.GetBusinessId();
            if (!businessIdNullable.HasValue)
            {
                throw new UnauthorizedAccessException("Business ID not found in token");
            }

            var businessId = businessIdNullable.Value;
            var giftCard = await _giftCardService.ValidateAndGetBalanceAsync(code, businessId);

            if (giftCard == null)
            {
                return NotFound(new { message = "Gift card not found" });
            }

            return Ok(giftCard);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving gift card");
            return StatusCode(500, new { message = "An error occurred while retrieving the gift card" });
        }
    }

    /// <summary>
    /// Create/issue a new gift card
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(GiftCardResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateGiftCard([FromBody] CreateGiftCardRequest request)
    {
        try
        {
            var businessIdNullable = User.GetBusinessId();
            if (!businessIdNullable.HasValue)
            {
                throw new UnauthorizedAccessException("Business ID not found in token");
            }

            var businessId = businessIdNullable.Value;

            if (request.OriginalAmount <= 0)
            {
                return BadRequest(new { message = "Original amount must be greater than zero" });
            }

            var giftCard = await _giftCardService.CreateGiftCardAsync(request, businessId);
            return CreatedAtAction(nameof(GetGiftCardByCode), new { code = giftCard.Code }, giftCard);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating gift card");
            return StatusCode(500, new { message = "An error occurred while creating the gift card" });
        }
    }

    /// <summary>
    /// Update gift card (balance, status, expiry)
    /// </summary>
    [HttpPatch("{code}")]
    [ProducesResponseType(typeof(GiftCardResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateGiftCard(string code, [FromBody] UpdateGiftCardRequest request)
    {
        try
        {
            var businessIdNullable = User.GetBusinessId();
            if (!businessIdNullable.HasValue)
            {
                throw new UnauthorizedAccessException("Business ID not found in token");
            }

            var businessId = businessIdNullable.Value;
            var giftCard = await _giftCardService.UpdateGiftCardAsync(code, request, businessId);

            if (giftCard == null)
            {
                return NotFound(new { message = "Gift card not found" });
            }

            return Ok(giftCard);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating gift card");
            return StatusCode(500, new { message = "An error occurred while updating the gift card" });
        }
    }

    /// <summary>
    /// Get all gift cards for the business
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<GiftCardResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllGiftCards([FromQuery] bool? activeOnly = null)
    {
        try
        {
            var businessIdNullable = User.GetBusinessId();
            if (!businessIdNullable.HasValue)
            {
                throw new UnauthorizedAccessException("Business ID not found in token");
            }

            var businessId = businessIdNullable.Value;
            var giftCards = await _giftCardService.GetAllGiftCardsAsync(businessId, activeOnly);
            return Ok(giftCards);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving gift cards");
            return StatusCode(500, new { message = "An error occurred while retrieving gift cards" });
        }
    }
}
