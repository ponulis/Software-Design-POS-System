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
    private readonly OrderValidationService _validationService;
    private readonly InventoryService? _inventoryService;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(
        ApplicationDbContext context, 
        PricingService pricingService, 
        OrderService orderService, 
        GiftCardService giftCardService,
        OrderValidationService validationService,
        ILogger<PaymentService> logger,
        StripeService? stripeService = null,
        InventoryService? inventoryService = null)
    {
        _context = context;
        _pricingService = pricingService;
        _orderService = orderService;
        _giftCardService = giftCardService;
        _validationService = validationService;
        _stripeService = stripeService;
        _inventoryService = inventoryService;
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

        // Validate order for payment using comprehensive validation service
        // Based on Section 3.1 of ORDER_MANAGEMENT_PLAN.md
        var validation = await _validationService.ValidateOrderForPaymentAsync(request.OrderId, businessId);
        if (!validation.IsValid)
        {
            throw new InvalidOperationException(validation.ErrorMessage ?? "Order validation failed");
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

        // Prevent duplicate payments
        await ValidatePaymentUniquenessAsync(request, paymentMethod, order.Id);

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
                // Check if StripeService is in mock mode
                if (_stripeService != null && _stripeService.IsMockMode)
                {
                    // Mock mode: create a mock payment intent
                    var mockPaymentIntent = await _stripeService.GetMockPaymentIntentAsync(
                        request.TransactionId, 
                        (long)(request.Amount * 100));
                    
                    // In mock mode, payment always succeeds
                    request.AuthorizationCode = mockPaymentIntent.LatestChargeId ?? mockPaymentIntent.Id;
                    _logger.LogInformation("Mock mode: Card payment processed successfully for PaymentIntentId={PaymentIntentId}", 
                        request.TransactionId);
                }
                else
                {
                    // Real Stripe mode
                    var paymentIntent = await _stripeService!.GetPaymentIntentAsync(request.TransactionId);

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

        // Audit logging: Log payment creation
        _logger.LogInformation(
            "Payment created: PaymentId={PaymentId}, OrderId={OrderId}, Amount={Amount}, Method={Method}, CreatedBy={CreatedBy}, PaidAt={PaidAt}, TransactionId={TransactionId}",
            payment.Id, payment.OrderId, payment.Amount, payment.Method, payment.CreatedBy, payment.PaidAt, payment.TransactionId);

        // Check if order should be marked as Paid
        await UpdateOrderStatusIfFullyPaidAsync(order);

        return MapToPaymentResponse(payment, cashReceived, change);
    }

    /// <summary>
    /// Validate payment uniqueness to prevent duplicate payments
    /// </summary>
    private async Task ValidatePaymentUniquenessAsync(CreatePaymentRequest request, PaymentMethod paymentMethod, int orderId)
    {
        // For card payments, check if PaymentIntentId (TransactionId) was already used
        if (paymentMethod == PaymentMethod.Card && !string.IsNullOrWhiteSpace(request.TransactionId))
        {
            var existingCardPayment = await _context.Payments
                .Where(p => p.OrderId == orderId && 
                           p.Method == PaymentMethod.Card && 
                           p.TransactionId == request.TransactionId)
                .FirstOrDefaultAsync();

            if (existingCardPayment != null)
            {
                throw new InvalidOperationException($"Payment with PaymentIntentId '{request.TransactionId}' already exists for this order. Duplicate payment prevented.");
            }
        }

        // For gift card payments, check if the same gift card code was used recently for this order
        // (Allow same gift card for different orders, but prevent duplicate use for same order)
        if (paymentMethod == PaymentMethod.GiftCard && !string.IsNullOrWhiteSpace(request.GiftCardCode))
        {
            var existingGiftCardPayment = await _context.Payments
                .Where(p => p.OrderId == orderId && 
                           p.Method == PaymentMethod.GiftCard && 
                           p.TransactionId == request.GiftCardCode)
                .FirstOrDefaultAsync();

            if (existingGiftCardPayment != null)
            {
                throw new InvalidOperationException($"Gift card '{request.GiftCardCode}' has already been used for this order. Duplicate payment prevented.");
            }
        }

        // For cash payments, we don't check duplicates since multiple cash payments are allowed
        // (e.g., customer pays in installments)
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

        // Validate order for payment using comprehensive validation service
        var validation = await _validationService.ValidateOrderForPaymentAsync(request.OrderId, businessId);
        if (!validation.IsValid)
        {
            throw new InvalidOperationException(validation.ErrorMessage ?? "Order validation failed");
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
            _logger.LogError("Split payment processing failed: OrderId={OrderId}, FailedCount={FailedCount}, Errors={Errors}",
                request.OrderId, failedPayments.Count, string.Join("; ", failedPayments));
            throw new InvalidOperationException($"Some split payments failed:\n{string.Join("\n", failedPayments)}");
        }

        // Audit logging: Log successful split payment creation
        _logger.LogInformation(
            "Split payments created successfully: OrderId={OrderId}, PaymentCount={PaymentCount}, TotalAmount={TotalAmount}",
            request.OrderId, createdPayments.Count, createdPayments.Sum(p => p.Amount));

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

    public async Task<PaginatedResponse<PaymentResponse>> GetAllPaymentsAsync(
        int businessId, 
        int? orderId = null,
        string? method = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int page = 1,
        int pageSize = 50)
    {
        var query = _context.Payments
            .Include(p => p.Order)
            .Where(p => p.Order.BusinessId == businessId);

        if (orderId.HasValue)
        {
            query = query.Where(p => p.OrderId == orderId.Value);
        }

        // Filter by payment method
        if (!string.IsNullOrWhiteSpace(method))
        {
            if (Enum.TryParse<PaymentMethod>(method, true, out var paymentMethod))
            {
                query = query.Where(p => p.Method == paymentMethod);
            }
        }

        // Filter by date range
        if (startDate.HasValue)
        {
            query = query.Where(p => p.PaidAt >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(p => p.PaidAt <= endDate.Value);
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply pagination
        var payments = await query
            .OrderByDescending(p => p.PaidAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var paymentResponses = payments.Select(p => MapToPaymentResponse(p)).ToList();

        return new PaginatedResponse<PaymentResponse>
        {
            Data = paymentResponses,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    /// <summary>
    /// Get payment history with audit information (CreatedBy user details)
    /// </summary>
    public async Task<List<PaymentHistoryResponse>> GetPaymentHistoryAsync(int businessId, int? orderId = null, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.Payments
            .Include(p => p.Order)
            .Include(p => p.Creator)
            .Where(p => p.Order.BusinessId == businessId);

        if (orderId.HasValue)
        {
            query = query.Where(p => p.OrderId == orderId.Value);
        }

        if (startDate.HasValue)
        {
            query = query.Where(p => p.PaidAt >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(p => p.PaidAt <= endDate.Value);
        }

        var payments = await query
            .OrderByDescending(p => p.PaidAt)
            .ToListAsync();

        return payments.Select(p => MapToPaymentHistoryResponse(p)).ToList();
    }

    private PaymentHistoryResponse MapToPaymentHistoryResponse(Payment payment)
    {
        decimal? cashReceived = null;
        decimal? change = null;

        // For cash payments, extract cashReceived and change
        if (payment.Method == PaymentMethod.Cash)
        {
            if (!string.IsNullOrEmpty(payment.TransactionId))
            {
                if (decimal.TryParse(payment.TransactionId, out var parsedCashReceived))
                {
                    cashReceived = parsedCashReceived;
                }
            }

            if (!string.IsNullOrEmpty(payment.AuthorizationCode))
            {
                if (decimal.TryParse(payment.AuthorizationCode, out var parsedChange))
                {
                    change = parsedChange;
                }
            }
        }

        return new PaymentHistoryResponse
        {
            PaymentId = payment.Id,
            OrderId = payment.OrderId,
            Amount = payment.Amount,
            Method = payment.Method.ToString(),
            PaidAt = payment.PaidAt,
            CreatedBy = payment.CreatedBy,
            CreatedByName = payment.Creator?.Name,
            TransactionId = payment.Method == PaymentMethod.Cash ? null : payment.TransactionId,
            AuthorizationCode = payment.Method == PaymentMethod.Cash ? null : payment.AuthorizationCode,
            CashReceived = cashReceived,
            Change = change
        };
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

        // Audit logging: Log payment deletion before removal
        _logger.LogWarning(
            "Payment deleted: PaymentId={PaymentId}, OrderId={OrderId}, Amount={Amount}, Method={Method}, CreatedBy={CreatedBy}, PaidAt={PaidAt}",
            payment.Id, payment.OrderId, payment.Amount, payment.Method, payment.CreatedBy, payment.PaidAt);

        _context.Payments.Remove(payment);
        await _context.SaveChangesAsync();

        // Recheck order status after payment deletion
        await UpdateOrderStatusIfFullyPaidAsync(payment.Order);

        return true;
    }

    /// <summary>
    /// Process refund for an order
    /// </summary>
    public async Task<RefundResponse> ProcessRefundAsync(int orderId, RefundRequest request, int businessId, int userId)
    {
        // Validate order exists and belongs to business
        var order = await _context.Orders
            .Where(o => o.Id == orderId && o.BusinessId == businessId)
            .Include(o => o.Payments)
            .FirstOrDefaultAsync();

        if (order == null)
        {
            throw new InvalidOperationException("Order not found or doesn't belong to your business");
        }

        // Validate order is paid
        if (order.Status != OrderStatus.Paid)
        {
            throw new InvalidOperationException($"Cannot refund order with status {order.Status}. Order must be Paid.");
        }

        // Calculate refund amount
        var refundAmount = request.Amount ?? order.Total;
        
        if (refundAmount <= 0)
        {
            throw new InvalidOperationException("Refund amount must be greater than zero");
        }

        if (refundAmount > order.Total)
        {
            throw new InvalidOperationException($"Refund amount ({refundAmount:C}) cannot exceed order total ({order.Total:C})");
        }

        var refundedPayments = new List<RefundPaymentResponse>();
        var remainingRefund = refundAmount;
        var paymentsToRefund = order.Payments.OrderByDescending(p => p.PaidAt).ToList();

        // Process refunds for each payment, starting with most recent
        foreach (var payment in paymentsToRefund)
        {
            if (remainingRefund <= 0)
            {
                break;
            }

            var paymentRefundAmount = Math.Min(remainingRefund, payment.Amount);
            var refundPayment = new RefundPaymentResponse
            {
                PaymentId = payment.Id,
                PaymentMethod = payment.Method.ToString(),
                RefundedAmount = paymentRefundAmount
            };

            try
            {
                switch (payment.Method)
                {
                    case PaymentMethod.Card:
                        // Process Stripe refund
                        if (_stripeService != null && !string.IsNullOrEmpty(payment.TransactionId))
                        {
                            try
                            {
                                var stripeRefund = await _stripeService.CreateRefundAsync(
                                    payment.TransactionId,
                                    paymentRefundAmount,
                                    request.Reason);

                                refundPayment.RefundTransactionId = stripeRefund.Id;
                                refundPayment.Status = "Success";
                                
                                _logger.LogInformation(
                                    "Stripe refund processed: PaymentId={PaymentId}, RefundId={RefundId}, Amount={Amount}",
                                    payment.Id, stripeRefund.Id, paymentRefundAmount);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Failed to process Stripe refund for PaymentId={PaymentId}", payment.Id);
                                refundPayment.Status = "Failed";
                                throw new InvalidOperationException($"Failed to process Stripe refund: {ex.Message}");
                            }
                        }
                        else
                        {
                            refundPayment.Status = "Pending"; // Manual refund required
                            _logger.LogWarning(
                                "Card payment refund requires manual processing: PaymentId={PaymentId}, TransactionId={TransactionId}",
                                payment.Id, payment.TransactionId);
                        }
                        break;

                    case PaymentMethod.GiftCard:
                        // Credit gift card balance
                        if (!string.IsNullOrEmpty(payment.TransactionId))
                        {
                            try
                            {
                                await _giftCardService.CreditGiftCardAsync(payment.TransactionId, paymentRefundAmount, businessId);
                                refundPayment.Status = "Success";
                                
                                _logger.LogInformation(
                                    "Gift card refund processed: PaymentId={PaymentId}, GiftCardCode={GiftCardCode}, Amount={Amount}",
                                    payment.Id, payment.TransactionId, paymentRefundAmount);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Failed to credit gift card for PaymentId={PaymentId}", payment.Id);
                                refundPayment.Status = "Failed";
                                throw new InvalidOperationException($"Failed to credit gift card: {ex.Message}");
                            }
                        }
                        else
                        {
                            refundPayment.Status = "Failed";
                            throw new InvalidOperationException("Gift card code not found for refund");
                        }
                        break;

                    case PaymentMethod.Cash:
                        // Cash refund - just record it (manual processing)
                        refundPayment.Status = "Success";
                        _logger.LogInformation(
                            "Cash refund recorded: PaymentId={PaymentId}, Amount={Amount}",
                            payment.Id, paymentRefundAmount);
                        break;
                }

                remainingRefund -= paymentRefundAmount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing refund for PaymentId={PaymentId}", payment.Id);
                refundPayment.Status = "Failed";
                throw;
            }

            refundedPayments.Add(refundPayment);
        }

        // Update order status
        if (refundAmount >= order.Total)
        {
            // Full refund - mark order as cancelled
            order.Status = OrderStatus.Cancelled;
        }
        else
        {
            // Partial refund - keep order as paid but log the refund
            // Note: In a real system, you might want to track partial refunds differently
        }

        order.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Audit logging
        _logger.LogInformation(
            "Refund processed: OrderId={OrderId}, RefundAmount={RefundAmount}, Reason={Reason}, RefundedPayments={RefundedPaymentsCount}",
            orderId, refundAmount, request.Reason ?? "N/A", refundedPayments.Count);

        return new RefundResponse
        {
            OrderId = orderId,
            RefundAmount = refundAmount,
            RefundMethod = refundAmount >= order.Total ? "Full" : "Partial",
            Reason = request.Reason,
            RefundedPayments = refundedPayments,
            OrderStatus = order.Status.ToString(),
            RefundedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Update order status to Paid if total payments equal or exceed order total
    /// Deducts inventory when order becomes fully paid
    /// </summary>
    private async Task UpdateOrderStatusIfFullyPaidAsync(Order order)
    {
        var totalPayments = await _context.Payments
            .Where(p => p.OrderId == order.Id)
            .SumAsync(p => p.Amount);

        if (totalPayments >= order.Total && order.Status != OrderStatus.Paid)
        {
            var previousStatus = order.Status;
            
            // Load order items with products for inventory deduction
            if (!_context.Entry(order).Collection(o => o.Items).IsLoaded)
            {
                await _context.Entry(order)
                    .Collection(o => o.Items)
                    .Query()
                    .Include(i => i.Product)
                    .LoadAsync();
            }

            order.Status = OrderStatus.Paid;
            order.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Deduct inventory when order is fully paid
            if (_inventoryService != null)
            {
                try
                {
                    await _inventoryService.DeductInventoryForOrderAsync(order);
                    _logger.LogInformation(
                        "Inventory deducted for OrderId={OrderId}", order.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, 
                        "Failed to deduct inventory for OrderId={OrderId}. Order is marked as Paid but inventory was not deducted.",
                        order.Id);
                    // Note: We don't throw here because the payment was successful
                    // In a production system, you might want to handle this differently
                }
            }

            // Audit logging: Log order status change to Paid
            _logger.LogInformation(
                "Order fully paid: OrderId={OrderId}, Total={Total}, TotalPayments={TotalPayments}, PreviousStatus={PreviousStatus}, NewStatus=Paid",
                order.Id, order.Total, totalPayments, previousStatus);
        }
        else if (totalPayments < order.Total && order.Status == OrderStatus.Paid)
        {
            // If payments were deleted and order is no longer fully paid, revert status
            order.Status = OrderStatus.Pending;
            order.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogWarning(
                "Order payment status reverted: OrderId={OrderId}, Total={Total}, TotalPayments={TotalPayments}, Status changed from Paid to Pending",
                order.Id, order.Total, totalPayments);
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
