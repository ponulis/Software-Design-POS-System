using backend.Data;
using backend.DTOs;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class PaymentService
{
    private readonly ApplicationDbContext _context;
    private readonly PricingService _pricingService;
    private readonly OrderService _orderService;
    private readonly GiftCardService _giftCardService;
    private readonly StripeService? _stripeService;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(
        ApplicationDbContext context, 
        PricingService pricingService, 
        OrderService orderService, 
        GiftCardService giftCardService,
        ILogger<PaymentService> logger,
        StripeService? stripeService = null)
    {
        _context = context;
        _pricingService = pricingService;
        _orderService = orderService;
        _giftCardService = giftCardService;
        _stripeService = stripeService;
        _logger = logger;
    }

    public async Task<PaymentResponse?> CreatePaymentAsync(CreatePaymentRequest request, int businessId, int userId)
    {
        // Validate order exists and belongs to business
        var order = await _context.Orders
            .Where(o => o.Id == request.OrderId && o.BusinessId == businessId)
            .Include(o => o.Items)
            .FirstOrDefaultAsync();

        if (order == null)
        {
            throw new InvalidOperationException("Order not found or doesn't belong to your business");
        }

        // Validate order status (can only pay Draft or Placed orders)
        if (order.Status != OrderStatus.Draft && order.Status != OrderStatus.Placed)
        {
            throw new InvalidOperationException($"Cannot create payment for order with status {order.Status}");
        }

        // Recalculate order totals using PricingService before creating payment
        // This ensures we have the latest tax and discount calculations
        var orderTotals = await _pricingService.CalculateOrderTotalsAsync(order, null);
        
        // Update order totals if they differ from calculated values
        if (order.SubTotal != orderTotals.SubTotal || 
            order.Tax != orderTotals.Tax || 
            order.Discount != orderTotals.Discount)
        {
            order.SubTotal = orderTotals.SubTotal;
            order.Tax = orderTotals.Tax;
            order.Discount = orderTotals.Discount;
            order.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        // Calculate remaining balance after existing payments
        var existingPayments = await _context.Payments
            .Where(p => p.OrderId == order.Id)
            .SumAsync(p => p.Amount);
        var remainingBalance = order.Total - existingPayments;

        // Validate that payment amount doesn't exceed remaining balance
        if (request.Amount > remainingBalance)
        {
            throw new InvalidOperationException($"Payment amount ({request.Amount:C}) exceeds remaining balance ({remainingBalance:C}). Order total: {order.Total:C}, Existing payments: {existingPayments:C}");
        }

        // Validate payment method
        if (!Enum.TryParse<PaymentMethod>(request.Method, true, out var paymentMethod))
        {
            throw new InvalidOperationException($"Invalid payment method: {request.Method}. Valid methods are: Cash, Card, GiftCard");
        }

        // Validate amount (must be positive)
        if (request.Amount <= 0)
        {
            throw new InvalidOperationException("Payment amount must be greater than zero");
        }

        // Cash payment specific validation
        decimal? change = null;
        decimal? cashReceived = null;
        if (paymentMethod == PaymentMethod.Cash)
        {
            // For cash payments, CashReceived is required
            if (!request.CashReceived.HasValue)
            {
                throw new InvalidOperationException("CashReceived is required for cash payments");
            }

            cashReceived = request.CashReceived.Value;

            // Validate cash received is positive
            if (cashReceived <= 0)
            {
                throw new InvalidOperationException("Cash received must be greater than zero");
            }

            // Validate cash received is sufficient
            if (cashReceived < remainingBalance)
            {
                throw new InvalidOperationException($"Insufficient cash. Order total: {order.Total:C}, Existing payments: {existingPayments:C}, Remaining: {remainingBalance:C}, Cash received: {cashReceived:C}");
            }

            // For cash payments, Amount should equal the remaining balance
            if (request.Amount > remainingBalance)
            {
                // If amount exceeds remaining balance, adjust it
                request.Amount = remainingBalance;
            }

            change = cashReceived - request.Amount;
        }
        else if (paymentMethod == PaymentMethod.Card)
        {
            // For card payments, validate Stripe is configured
            if (_stripeService == null)
            {
                throw new InvalidOperationException("Stripe payment service is not configured");
            }

            // For card payments, TransactionId should contain PaymentIntentId
            if (string.IsNullOrWhiteSpace(request.TransactionId))
            {
                throw new InvalidOperationException("PaymentIntentId (TransactionId) is required for card payments");
            }

            // Confirm the Stripe payment intent
            try
            {
                var paymentIntent = await _stripeService.GetPaymentIntentAsync(request.TransactionId);

                // Validate payment intent status
                if (paymentIntent.Status != "succeeded" && paymentIntent.Status != "processing")
                {
                    throw new InvalidOperationException($"Payment intent status is {paymentIntent.Status}. Payment must be succeeded or processing.");
                }

                // Validate payment amount matches
                var stripeAmount = paymentIntent.Amount / 100m; // Convert from cents
                if (Math.Abs(stripeAmount - request.Amount) > 0.01m)
                {
                    throw new InvalidOperationException($"Payment amount mismatch. Stripe: {stripeAmount:C}, Request: {request.Amount:C}");
                }

                // Store Stripe metadata
                request.AuthorizationCode = paymentIntent.LatestChargeId ?? paymentIntent.Id;
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Stripe payment");
                throw new InvalidOperationException($"Card payment failed: {ex.Message}");
            }
        }
        else if (paymentMethod == PaymentMethod.GiftCard)
        {
            // For gift card payments, GiftCardCode is required
            if (string.IsNullOrWhiteSpace(request.GiftCardCode))
            {
                throw new InvalidOperationException("GiftCardCode is required for gift card payments");
            }

            // Validate and deduct from gift card
            try
            {
                await _giftCardService.DeductBalanceAsync(request.GiftCardCode, request.Amount, businessId);
                
                // Store gift card code in TransactionId for gift card payments
                request.TransactionId = request.GiftCardCode;
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException($"Gift card payment failed: {ex.Message}");
            }
        }

        // Create payment record
        var payment = new Payment
        {
            OrderId = request.OrderId,
            CreatedBy = userId,
            Amount = request.Amount,
            Method = paymentMethod,
            PaidAt = DateTime.UtcNow,
            TransactionId = paymentMethod == PaymentMethod.Cash && cashReceived.HasValue 
                ? cashReceived.Value.ToString("F2") // Store cashReceived in TransactionId for cash payments
                : request.TransactionId,
            AuthorizationCode = paymentMethod == PaymentMethod.Cash && change.HasValue
                ? change.Value.ToString("F2") // Store change in AuthorizationCode for cash payments
                : request.AuthorizationCode
        };

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        // Check if order should be marked as Paid
        await UpdateOrderStatusIfFullyPaidAsync(order);

        return MapToPaymentResponse(payment, cashReceived, change);
    }

    /// <summary>
    /// Get order for payment validation (used by Stripe controller)
    /// </summary>
    public async Task<Order?> GetOrderForPaymentAsync(int orderId, int businessId)
    {
        return await _context.Orders
            .Where(o => o.Id == orderId && o.BusinessId == businessId)
            .Include(o => o.Items)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Create split payments (multiple payments for one order)
    /// </summary>
    public async Task<SplitPaymentResponse> CreateSplitPaymentsAsync(CreateSplitPaymentRequest request, int businessId, int userId)
    {
        // Validate order exists and belongs to business
        var order = await _context.Orders
            .Where(o => o.Id == request.OrderId && o.BusinessId == businessId)
            .Include(o => o.Items)
            .FirstOrDefaultAsync();

        if (order == null)
        {
            throw new InvalidOperationException("Order not found or doesn't belong to your business");
        }

        // Validate order status (can only pay Draft or Placed orders)
        if (order.Status != OrderStatus.Draft && order.Status != OrderStatus.Placed)
        {
            throw new InvalidOperationException($"Cannot create payments for order with status {order.Status}");
        }

        // Recalculate order totals using PricingService
        var orderTotals = await _pricingService.CalculateOrderTotalsAsync(order, null);
        
        // Update order totals if they differ from calculated values
        if (order.SubTotal != orderTotals.SubTotal || 
            order.Tax != orderTotals.Tax || 
            order.Discount != orderTotals.Discount)
        {
            order.SubTotal = orderTotals.SubTotal;
            order.Tax = orderTotals.Tax;
            order.Discount = orderTotals.Discount;
            order.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        // Calculate existing payments
        var existingPayments = await _context.Payments
            .Where(p => p.OrderId == order.Id)
            .SumAsync(p => p.Amount);
        var remainingBalance = order.Total - existingPayments;

        // Validate split payments
        if (request.Payments == null || !request.Payments.Any())
        {
            throw new InvalidOperationException("At least one payment is required for split payment");
        }

        // Validate all payment amounts are positive
        foreach (var splitPayment in request.Payments)
        {
            if (splitPayment.Amount <= 0)
            {
                throw new InvalidOperationException("All payment amounts must be greater than zero");
            }
        }

        // Calculate total of split payments
        var splitTotal = request.Payments.Sum(p => p.Amount);

        // Validate sum of split payments equals remaining balance (with small tolerance for rounding)
        if (Math.Abs(splitTotal - remainingBalance) > 0.01m)
        {
            throw new InvalidOperationException($"Sum of split payments ({splitTotal:C}) must equal remaining balance ({remainingBalance:C}). Order total: {order.Total:C}, Existing payments: {existingPayments:C}");
        }

        // Process each payment individually
        var createdPayments = new List<PaymentResponse>();
        var failedPayments = new List<string>();

        foreach (var splitPayment in request.Payments)
        {
            try
            {
                // Create payment request for this split
                var paymentRequest = new CreatePaymentRequest
                {
                    OrderId = request.OrderId,
                    Amount = splitPayment.Amount,
                    Method = splitPayment.Method,
                    CashReceived = splitPayment.CashReceived,
                    GiftCardCode = splitPayment.GiftCardCode,
                    TransactionId = splitPayment.PaymentIntentId
                };

                // Create the payment
                var paymentResponse = await CreatePaymentAsync(paymentRequest, businessId, userId);
                if (paymentResponse != null)
                {
                    createdPayments.Add(paymentResponse);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating split payment");
                failedPayments.Add($"Payment {splitPayment.Method} for {splitPayment.Amount:C}: {ex.Message}");
            }
        }

        // If any payments failed, throw error with details
        if (failedPayments.Any())
        {
            throw new InvalidOperationException($"Some split payments failed:\n{string.Join("\n", failedPayments)}");
        }

        // Get updated order details
        var orderResponse = await _orderService.GetOrderByIdAsync(request.OrderId, businessId);
        if (orderResponse == null)
        {
            throw new InvalidOperationException("Order not found after split payment creation");
        }

        // Calculate total paid and remaining balance
        var totalPaid = existingPayments + createdPayments.Sum(p => p.Amount);
        var finalRemainingBalance = order.Total - totalPaid;
        var isFullyPaid = orderResponse.Status == "Paid";

        return new SplitPaymentResponse
        {
            OrderId = request.OrderId,
            Payments = createdPayments,
            TotalPaid = totalPaid,
            RemainingBalance = finalRemainingBalance,
            IsFullyPaid = isFullyPaid,
            Order = orderResponse
        };
    }

    /// <summary>
    /// Create payment and return confirmation with order details
    /// </summary>
    public async Task<PaymentConfirmationResponse> CreatePaymentWithConfirmationAsync(CreatePaymentRequest request, int businessId, int userId)
    {
        var paymentResponse = await CreatePaymentAsync(request, businessId, userId);
        
        if (paymentResponse == null)
        {
            throw new InvalidOperationException("Failed to create payment");
        }

        // Get updated order details
        var orderResponse = await _orderService.GetOrderByIdAsync(request.OrderId, businessId);
        
        if (orderResponse == null)
        {
            throw new InvalidOperationException("Order not found after payment creation");
        }

        var isFullyPaid = orderResponse.Status == "Paid";
        var message = isFullyPaid 
            ? "Payment successful. Order has been fully paid."
            : $"Payment of {paymentResponse.Amount:C} recorded. Remaining balance: {orderResponse.Total - orderResponse.SubTotal + orderResponse.Discount - orderResponse.Tax:C}";

        return new PaymentConfirmationResponse
        {
            Payment = paymentResponse,
            Order = orderResponse,
            Message = message
        };
    }

    public async Task<List<PaymentResponse>> GetAllPaymentsAsync(int businessId, int? orderId = null)
    {
        var query = _context.Payments
            .Include(p => p.Order)
            .Where(p => p.Order.BusinessId == businessId);

        if (orderId.HasValue)
        {
            query = query.Where(p => p.OrderId == orderId.Value);
        }

        var payments = await query
            .OrderByDescending(p => p.PaidAt)
            .ToListAsync();

        return payments.Select(p => MapToPaymentResponse(p)).ToList();
    }

    public async Task<PaymentResponse?> GetPaymentByIdAsync(int paymentId, int businessId)
    {
        var payment = await _context.Payments
            .Include(p => p.Order)
            .Where(p => p.Id == paymentId && p.Order.BusinessId == businessId)
            .FirstOrDefaultAsync();

        return payment != null ? MapToPaymentResponse(payment) : null;
    }

    public async Task<bool> DeletePaymentAsync(int paymentId, int businessId)
    {
        var payment = await _context.Payments
            .Include(p => p.Order)
            .Where(p => p.Id == paymentId && p.Order.BusinessId == businessId)
            .FirstOrDefaultAsync();

        if (payment == null)
        {
            return false;
        }

        // Cannot delete payment if order is already paid (would need refund logic)
        if (payment.Order.Status == OrderStatus.Paid)
        {
            throw new InvalidOperationException("Cannot delete payment for a paid order. Process a refund instead.");
        }

        _context.Payments.Remove(payment);
        await _context.SaveChangesAsync();

        // Recheck order status after payment deletion
        await UpdateOrderStatusIfFullyPaidAsync(payment.Order);

        return true;
    }

    /// <summary>
    /// Update order status to Paid if total payments equal or exceed order total
    /// </summary>
    private async Task UpdateOrderStatusIfFullyPaidAsync(Order order)
    {
        var totalPayments = await _context.Payments
            .Where(p => p.OrderId == order.Id)
            .SumAsync(p => p.Amount);

        if (totalPayments >= order.Total && order.Status != OrderStatus.Paid)
        {
            order.Status = OrderStatus.Paid;
            order.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    private PaymentResponse MapToPaymentResponse(Payment payment, decimal? cashReceived = null, decimal? change = null)
    {
        // For cash payments, try to extract cashReceived and change from stored fields
        if (payment.Method == PaymentMethod.Cash)
        {
            // If not provided, try to parse from TransactionId and AuthorizationCode
            if (!cashReceived.HasValue && !string.IsNullOrEmpty(payment.TransactionId))
            {
                if (decimal.TryParse(payment.TransactionId, out var parsedCashReceived))
                {
                    cashReceived = parsedCashReceived;
                }
            }

            if (!change.HasValue && !string.IsNullOrEmpty(payment.AuthorizationCode))
            {
                if (decimal.TryParse(payment.AuthorizationCode, out var parsedChange))
                {
                    change = parsedChange;
                }
            }
        }

        return new PaymentResponse
        {
            Id = payment.Id,
            OrderId = payment.OrderId,
            CreatedBy = payment.CreatedBy,
            Amount = payment.Amount,
            Method = payment.Method.ToString(),
            CashReceived = cashReceived,
            Change = change,
            PaidAt = payment.PaidAt,
            // For cash payments, TransactionId and AuthorizationCode are used for cashReceived/change
            // For other payment methods, they contain actual transaction/authorization codes
            TransactionId = payment.Method == PaymentMethod.Cash ? null : payment.TransactionId,
            AuthorizationCode = payment.Method == PaymentMethod.Cash ? null : payment.AuthorizationCode
        };
    }
}
