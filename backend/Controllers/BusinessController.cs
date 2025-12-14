using backend.DTOs;
using backend.Extensions;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/business")]
[Authorize]
public class BusinessController : ControllerBase
{
    private readonly BusinessService _businessService;
    private readonly ILogger<BusinessController> _logger;

    public BusinessController(BusinessService businessService, ILogger<BusinessController> logger)
    {
        _businessService = businessService;
        _logger = logger;
    }

    /// <summary>
    /// Get business details for the authenticated user's business
    /// </summary>
    /// <returns>Business information</returns>
    [HttpGet]
    [ProducesResponseType(typeof(BusinessResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBusiness()
    {
        try
        {
            var businessIdNullable = User.GetBusinessId();
            if (!businessIdNullable.HasValue)
            {
                throw new UnauthorizedAccessException("Business ID not found in token");
            }

            var businessId = businessIdNullable.Value;
            var business = await _businessService.GetBusinessByIdAsync(businessId);

            if (business == null)
            {
                return NotFound(new { message = "Business not found" });
            }

            return Ok(business);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving business");
            return StatusCode(500, new { message = "An error occurred while retrieving business information" });
        }
    }

    /// <summary>
    /// Update business settings (PATCH per API contract)
    /// </summary>
    /// <param name="request">Business fields to modify</param>
    /// <returns>Updated business information</returns>
    /// <remarks>
    /// Sample request:
    /// 
    ///     PATCH /api/business
    ///     {
    ///         "name": "Updated Business Name",
    ///         "description": "Updated description",
    ///         "address": "123 New Street",
    ///         "contactEmail": "newemail@example.com",
    ///         "phoneNumber": "+1234567890"
    ///     }
    /// </remarks>
    [HttpPatch]
    [ProducesResponseType(typeof(BusinessResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateBusiness([FromBody] UpdateBusinessRequest request)
    {
        try
        {
            var businessIdNullable = User.GetBusinessId();
            if (!businessIdNullable.HasValue)
            {
                throw new UnauthorizedAccessException("Business ID not found in token");
            }

            var businessId = businessIdNullable.Value;
            var business = await _businessService.UpdateBusinessAsync(businessId, request);

            if (business == null)
            {
                return NotFound(new { message = "Business not found" });
            }

            return Ok(business);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating business");
            return StatusCode(500, new { message = "An error occurred while updating business information" });
        }
    }
}
