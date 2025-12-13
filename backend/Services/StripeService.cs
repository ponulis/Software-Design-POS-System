using backend.DTOs;
using Stripe;

namespace backend.Services;

public class StripeService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<StripeService> _logger;

    public StripeService(IConfiguration configuration, ILogger<StripeService> logger)
    {
        _configuration = configuration;
        _logger = logger;

        var secretKey = _configuration["StripeSettings:SecretKey"];
        if (!string.IsNullOrEmpty(secretKey))
        {
            StripeConfiguration.ApiKey = secretKey;
        }
    }

    /// <summary>
    /// Create a Stripe payment intent
    /// </summary>
    public async Task<StripePaymentIntentResponse> CreatePaymentIntentAsync(CreateStripePaymentIntentRequest request)
    {
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
}
