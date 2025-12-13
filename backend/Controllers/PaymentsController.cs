using backend.DTOs;
using backend.Extensions;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/payments")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly PaymentService _paymentService;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(PaymentService paymentService, ILogger<PaymentsController> logger)
    {
        _paymentService = paymentService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new payment
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(PaymentConfirmationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentRequest request)
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

            // Create payment and return confirmation with order details
            var confirmation = await _paymentService.CreatePaymentWithConfirmationAsync(request, businessId, userId);
            return CreatedAtAction(nameof(GetPaymentById), new { paymentId = confirmation.Payment.Id }, confirmation);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating payment");
            return StatusCode(500, new { message = "An error occurred while creating the payment" });
        }
    }

    /// <summary>
    /// Get all payments for the authenticated user's business
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<PaymentResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllPayments([FromQuery] int? orderId = null)
    {
        try
        {
            var businessIdNullable = User.GetBusinessId();
            if (!businessIdNullable.HasValue)
            {
                throw new UnauthorizedAccessException("Business ID not found in token");
            }

            var businessId = businessIdNullable.Value;
            var payments = await _paymentService.GetAllPaymentsAsync(businessId, orderId);
            return Ok(payments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving payments");
            return StatusCode(500, new { message = "An error occurred while retrieving payments" });
        }
    }

    /// <summary>
    /// Get payment by ID
    /// </summary>
    [HttpGet("{paymentId}")]
    [ProducesResponseType(typeof(PaymentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPaymentById(int paymentId)
    {
        try
        {
            var businessIdNullable = User.GetBusinessId();
            if (!businessIdNullable.HasValue)
            {
                throw new UnauthorizedAccessException("Business ID not found in token");
            }

            var businessId = businessIdNullable.Value;
            var payment = await _paymentService.GetPaymentByIdAsync(paymentId, businessId);

            if (payment == null)
            {
                return NotFound(new { message = "Payment not found" });
            }

            return Ok(payment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving payment");
            return StatusCode(500, new { message = "An error occurred while retrieving the payment" });
        }
    }

    /// <summary>
    /// Get payment history with audit information
    /// </summary>
    [HttpGet("history")]
    [ProducesResponseType(typeof(List<PaymentHistoryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaymentHistory(
        [FromQuery] int? orderId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var businessIdNullable = User.GetBusinessId();
            if (!businessIdNullable.HasValue)
            {
                throw new UnauthorizedAccessException("Business ID not found in token");
            }

            var businessId = businessIdNullable.Value;
            var paymentHistory = await _paymentService.GetPaymentHistoryAsync(businessId, orderId, startDate, endDate);
            return Ok(paymentHistory);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving payment history");
            return StatusCode(500, new { message = "An error occurred while retrieving payment history" });
        }
    }

    /// <summary>
    /// Create split payments (multiple payments for one order)
    /// </summary>
    [HttpPost("split")]
    [ProducesResponseType(typeof(SplitPaymentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateSplitPayments([FromBody] CreateSplitPaymentRequest request)
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

            var splitPaymentResponse = await _paymentService.CreateSplitPaymentsAsync(request, businessId, userId);
            return Ok(splitPaymentResponse);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating split payments");
            return StatusCode(500, new { message = "An error occurred while creating split payments" });
        }
    }

    /// <summary>
    /// Delete a payment
    /// </summary>
    [HttpDelete("{paymentId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePayment(int paymentId)
    {
        try
        {
            var businessIdNullable = User.GetBusinessId();
            if (!businessIdNullable.HasValue)
            {
                throw new UnauthorizedAccessException("Business ID not found in token");
            }

            var businessId = businessIdNullable.Value;
            var deleted = await _paymentService.DeletePaymentAsync(paymentId, businessId);

            if (!deleted)
            {
                return NotFound(new { message = "Payment not found" });
            }

            return Ok(new { message = "Payment deleted successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting payment");
            return StatusCode(500, new { message = "An error occurred while deleting the payment" });
        }
    }
}
