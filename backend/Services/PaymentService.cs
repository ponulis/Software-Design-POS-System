using backend.Data;
using backend.DTOs;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class PaymentService
{
    private readonly ApplicationDbContext _context;

    public PaymentService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaymentResponse?> CreatePaymentAsync(CreatePaymentRequest request, int businessId, int userId)
    {
        // Validate order exists and belongs to business
        var order = await _context.Orders
            .Where(o => o.Id == request.OrderId && o.BusinessId == businessId)
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

            // Calculate remaining order balance (considering existing payments)
            var existingPayments = await _context.Payments
                .Where(p => p.OrderId == order.Id)
                .SumAsync(p => p.Amount);
            var remainingBalance = order.Total - existingPayments;

            // Validate cash received is sufficient
            if (cashReceived < remainingBalance)
            {
                throw new InvalidOperationException($"Insufficient cash. Order total: {order.Total:C}, Existing payments: {existingPayments:C}, Remaining: {remainingBalance:C}, Cash received: {cashReceived:C}");
            }

            // Calculate change (cashReceived - remainingBalance)
            // Note: For cash payments, Amount should equal the remaining balance or less
            if (request.Amount > remainingBalance)
            {
                // If amount exceeds remaining balance, adjust it
                request.Amount = remainingBalance;
            }

            change = cashReceived - request.Amount;
        }
        else
        {
            // For non-cash payments, amount should match order total (or remaining balance)
            var existingPayments = await _context.Payments
                .Where(p => p.OrderId == order.Id)
                .SumAsync(p => p.Amount);
            var remainingBalance = order.Total - existingPayments;

            // Validate amount doesn't exceed remaining balance
            if (request.Amount > remainingBalance)
            {
                throw new InvalidOperationException($"Payment amount ({request.Amount:C}) exceeds remaining balance ({remainingBalance:C})");
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
