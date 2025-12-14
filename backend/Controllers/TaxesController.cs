using backend.DTOs;
using backend.Extensions;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/taxes")]
[Authorize]
public class TaxesController : ControllerBase
{
    private readonly TaxService _taxService;
    private readonly ILogger<TaxesController> _logger;

    public TaxesController(TaxService taxService, ILogger<TaxesController> logger)
    {
        _taxService = taxService;
        _logger = logger;
    }

    /// <summary>
    /// Get all tax rates for the authenticated user's business
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<TaxResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllTaxes([FromQuery] bool? activeOnly = null)
    {
        try
        {
            var businessIdNullable = User.GetBusinessId();
            if (!businessIdNullable.HasValue)
            {
                throw new UnauthorizedAccessException("Business ID not found in token");
            }

            var businessId = businessIdNullable.Value;
            var taxes = await _taxService.GetAllTaxesAsync(businessId, activeOnly);
            return Ok(taxes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving taxes");
            return StatusCode(500, new { message = "An error occurred while retrieving taxes" });
        }
    }

    /// <summary>
    /// Get tax by ID
    /// </summary>
    [HttpGet("{taxId}")]
    [ProducesResponseType(typeof(TaxResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTaxById(int taxId)
    {
        try
        {
            var businessIdNullable = User.GetBusinessId();
            if (!businessIdNullable.HasValue)
            {
                throw new UnauthorizedAccessException("Business ID not found in token");
            }

            var businessId = businessIdNullable.Value;
            var tax = await _taxService.GetTaxByIdAsync(taxId, businessId);

            if (tax == null)
            {
                return NotFound(new { message = "Tax not found" });
            }

            return Ok(tax);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tax");
            return StatusCode(500, new { message = "An error occurred while retrieving the tax" });
        }
    }

    /// <summary>
    /// Create a new tax rule
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(TaxResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateTax([FromBody] CreateTaxRequest request)
    {
        try
        {
            var businessIdNullable = User.GetBusinessId();
            if (!businessIdNullable.HasValue)
            {
                throw new UnauthorizedAccessException("Business ID not found in token");
            }

            var businessId = businessIdNullable.Value;
            var tax = await _taxService.CreateTaxAsync(request, businessId);
            return CreatedAtAction(nameof(GetTaxById), new { taxId = tax.Id }, tax);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating tax");
            return StatusCode(500, new { message = "An error occurred while creating the tax" });
        }
    }

    /// <summary>
    /// Update tax rule (PATCH per API contract)
    /// </summary>
    [HttpPatch("{taxId}")]
    [ProducesResponseType(typeof(TaxResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTax(int taxId, [FromBody] UpdateTaxRequest request)
    {
        try
        {
            var businessIdNullable = User.GetBusinessId();
            if (!businessIdNullable.HasValue)
            {
                throw new UnauthorizedAccessException("Business ID not found in token");
            }

            var businessId = businessIdNullable.Value;
            var tax = await _taxService.UpdateTaxAsync(taxId, request, businessId);

            if (tax == null)
            {
                return NotFound(new { message = "Tax not found" });
            }

            return Ok(tax);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating tax");
            return StatusCode(500, new { message = "An error occurred while updating the tax" });
        }
    }

    /// <summary>
    /// Delete tax rule
    /// </summary>
    [HttpDelete("{taxId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTax(int taxId)
    {
        try
        {
            var businessIdNullable = User.GetBusinessId();
            if (!businessIdNullable.HasValue)
            {
                throw new UnauthorizedAccessException("Business ID not found in token");
            }

            var businessId = businessIdNullable.Value;
            var deleted = await _taxService.DeleteTaxAsync(taxId, businessId);

            if (!deleted)
            {
                return NotFound(new { message = "Tax not found" });
            }

            return Ok(new { message = "Tax deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting tax");
            return StatusCode(500, new { message = "An error occurred while deleting the tax" });
        }
    }
}
