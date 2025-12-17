using backend.DTOs;
using Stripe;

namespace backend.Services;

/// <summary>
/// Mock PaymentIntent for testing when Stripe is not configured
/// </summary>
public class MockPaymentIntent
{
    public string Id { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public long Amount { get; set; }
    public string? LatestChargeId { get; set; }
}

public class StripeService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<StripeService> _logger;
    private readonly bool _isMockMode;

    public StripeService(IConfiguration configuration, ILogger<StripeService> logger)
    {
        _configuration = configuration;
        _logger = logger;

        var secretKey = _configuration["StripeSettings:SecretKey"];
        if (!string.IsNullOrEmpty(secretKey))
        {
            StripeConfiguration.ApiKey = secretKey;
            _isMockMode = false;
        }
        else
        {
            _isMockMode = true;
            _logger.LogWarning("Stripe secret key not configured. Running in mock mode - all payments will succeed.");
        }
    }

    /// <summary>
    /// Create a Stripe payment intent
    /// </summary>
    public async Task<StripePaymentIntentResponse> CreatePaymentIntentAsync(CreateStripePaymentIntentRequest request)
    {
        // Mock mode: return a mock payment intent
        if (_isMockMode)
        {
            _logger.LogInformation("Mock mode: Creating mock payment intent for OrderId={OrderId}, Amount={Amount}",
                request.OrderId, request.Amount);
            
            await Task.Delay(100); // Simulate async operation
            
            var mockPaymentIntentId = $"pi_mock_{Guid.NewGuid():N}";
            return new StripePaymentIntentResponse
            {
                ClientSecret = $"pi_mock_{Guid.NewGuid():N}_secret_{Guid.NewGuid():N}",
                PaymentIntentId = mockPaymentIntentId,
                Status = "requires_confirmation"
            };
        }

        try
        {
            var service = new PaymentIntentService();
            
            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(request.Amount * 100), // Convert to cents
                Currency = request.Currency.ToLower(),
                PaymentMethodTypes = new List<string> { "card" },
                Metadata = new Dictionary<string, string>
                {
                    { "order_id", request.OrderId.ToString() }
                }
            };

            // If payment method is provided, attach it
            if (!string.IsNullOrEmpty(request.PaymentMethodId))
            {
                options.PaymentMethod = request.PaymentMethodId;
                options.ConfirmationMethod = "manual";
                options.Confirm = true;
            }

            var paymentIntent = await service.CreateAsync(options);

            return new StripePaymentIntentResponse
            {
                ClientSecret = paymentIntent.ClientSecret,
                PaymentIntentId = paymentIntent.Id,
                Status = paymentIntent.Status
            };
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error creating payment intent");
            throw new InvalidOperationException($"Stripe payment failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Confirm a Stripe payment intent
    /// </summary>
    public async Task<PaymentIntent> ConfirmPaymentIntentAsync(string paymentIntentId)
    {
        try
        {
            var service = new PaymentIntentService();
            var paymentIntent = await service.ConfirmAsync(paymentIntentId);

            return paymentIntent;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error confirming payment intent");
            throw new InvalidOperationException($"Stripe payment confirmation failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Retrieve a Stripe payment intent
    /// </summary>
    public async Task<PaymentIntent> GetPaymentIntentAsync(string paymentIntentId)
    {
        // Mock mode: return a mock payment intent with succeeded status
        if (_isMockMode)
        {
            _logger.LogInformation("Mock mode: Retrieving mock payment intent {PaymentIntentId}", paymentIntentId);
            
            await Task.Delay(50); // Simulate async operation
            
            // Create a mock PaymentIntent object using reflection or return a minimal object
            // Since PaymentIntent is from Stripe SDK, we'll create a mock response
            // For mock mode, we'll assume the payment succeeded
            throw new InvalidOperationException("Mock mode: Use GetMockPaymentIntentAsync instead");
        }

        try
        {
            var service = new PaymentIntentService();
            var paymentIntent = await service.GetAsync(paymentIntentId);

            return paymentIntent;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error retrieving payment intent");
            throw new InvalidOperationException($"Failed to retrieve payment intent: {ex.Message}");
        }
    }

    /// <summary>
    /// Check if service is in mock mode
    /// </summary>
    public bool IsMockMode => _isMockMode;

    /// <summary>
    /// Get mock payment intent for testing (used in mock mode)
    /// </summary>
    public async Task<MockPaymentIntent> GetMockPaymentIntentAsync(string paymentIntentId, long amountInCents)
    {
        if (!_isMockMode)
        {
            throw new InvalidOperationException("This method is only available in mock mode");
        }

        await Task.Delay(50);
        
        return new MockPaymentIntent
        {
            Id = paymentIntentId,
            Status = "succeeded",
            Amount = amountInCents,
            LatestChargeId = $"ch_mock_{Guid.NewGuid():N}"
        };
    }

    /// <summary>
    /// Cancel a Stripe payment intent
    /// </summary>
    public async Task<PaymentIntent> CancelPaymentIntentAsync(string paymentIntentId)
    {
        try
        {
            var service = new PaymentIntentService();
            var paymentIntent = await service.CancelAsync(paymentIntentId);

            return paymentIntent;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error canceling payment intent");
            throw new InvalidOperationException($"Failed to cancel payment intent: {ex.Message}");
        }
    }

    /// <summary>
    /// Create a Stripe refund for a payment intent
    /// </summary>
    public async Task<Refund> CreateRefundAsync(string paymentIntentId, decimal? amount = null, string? reason = null)
    {
        try
        {
            var service = new RefundService();
            
            var options = new RefundCreateOptions
            {
                PaymentIntent = paymentIntentId,
                Reason = reason != null ? MapRefundReason(reason) : null
            };

            if (amount.HasValue)
            {
                options.Amount = (long)(amount.Value * 100); // Convert to cents
            }

            var refund = await service.CreateAsync(options);

            return refund;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error creating refund");
            throw new InvalidOperationException($"Stripe refund failed: {ex.Message}");
        }
    }

    private static string MapRefundReason(string? reason)
    {
        // Map common refund reasons to Stripe refund reasons
        if (string.IsNullOrWhiteSpace(reason))
        {
            return "requested_by_customer";
        }

        var lowerReason = reason.ToLower();
        if (lowerReason.Contains("duplicate") || lowerReason.Contains("fraud"))
        {
            return "fraudulent";
        }
        if (lowerReason.Contains("cancel") || lowerReason.Contains("return"))
        {
            return "requested_by_customer";
        }

        return "requested_by_customer";
    }
}
