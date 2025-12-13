using backend.DTOs;
using backend.Extensions;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/stripe")]
[Authorize]
public class StripeController : ControllerBase
{
    private readonly StripeService _stripeService;
    private readonly PaymentService _paymentService;
    private readonly ILogger<StripeController> _logger;

    public StripeController(
        StripeService stripeService, 
        PaymentService paymentService,
        ILogger<StripeController> logger)
    {
        _stripeService = stripeService;
        _paymentService = paymentService;
        _logger = logger;
    }

    /// <summary>
    /// Create a Stripe payment intent for card payment
    /// </summary>
    [HttpPost("create-payment-intent")]
    [ProducesResponseType(typeof(StripePaymentIntentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreatePaymentIntent([FromBody] CreateStripePaymentIntentRequest request)
    {
        try
        {
            var businessIdNullable = User.GetBusinessId();
            if (!businessIdNullable.HasValue)
            {
                throw new UnauthorizedAccessException("Business ID not found in token");
            }

            var businessId = businessIdNullable.Value;

            // Validate order exists and belongs to business
            var order = await _paymentService.GetOrderForPaymentAsync(request.OrderId, businessId);
            if (order == null)
            {
                return BadRequest(new { message = "Order not found or doesn't belong to your business" });
            }

            // Use order total for payment amount
            if (request.Amount <= 0)
            {
                request.Amount = order.Total;
            }

            var paymentIntent = await _stripeService.CreatePaymentIntentAsync(request);
            return Ok(paymentIntent);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Stripe payment intent");
            return StatusCode(500, new { message = "An error occurred while creating payment intent" });
        }
    }

    /// <summary>
    /// Confirm a Stripe payment and create payment record
    /// </summary>
    [HttpPost("confirm-payment")]
    [ProducesResponseType(typeof(PaymentConfirmationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConfirmPayment([FromBody] ConfirmStripePaymentRequest request)
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

            // Get payment intent from Stripe
            var paymentIntent = await _stripeService.GetPaymentIntentAsync(request.PaymentIntentId);

            // Validate payment intent is succeeded
            if (paymentIntent.Status != "succeeded")
            {
                return BadRequest(new { message = $"Payment intent status is {paymentIntent.Status}. Payment must be succeeded." });
            }

            // Create payment record
            var paymentRequest = new CreatePaymentRequest
            {
                OrderId = request.OrderId,
                Amount = paymentIntent.Amount / 100m, // Convert from cents
                Method = "Card",
                TransactionId = paymentIntent.Id,
                AuthorizationCode = paymentIntent.LatestChargeId ?? paymentIntent.Id
            };

            var confirmation = await _paymentService.CreatePaymentWithConfirmationAsync(paymentRequest, businessId, userId);
            return Ok(confirmation);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming Stripe payment");
            return StatusCode(500, new { message = "An error occurred while confirming payment" });
        }
    }
}
