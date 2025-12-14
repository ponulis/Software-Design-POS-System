using backend.Data;
using backend.DTOs;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class AnalyticsService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AnalyticsService> _logger;

    public AnalyticsService(ApplicationDbContext context, ILogger<AnalyticsService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<DashboardResponse> GetDashboardDataAsync(int businessId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var now = DateTime.UtcNow;
        var today = now.Date;
        var weekStart = today.AddDays(-(int)today.DayOfWeek);
        var monthStart = new DateTime(today.Year, today.Month, 1);

        // Base query for orders
        var ordersQuery = _context.Orders
            .Where(o => o.BusinessId == businessId);

        if (startDate.HasValue)
        {
            ordersQuery = ordersQuery.Where(o => o.CreatedAt >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            ordersQuery = ordersQuery.Where(o => o.CreatedAt <= endDate.Value);
        }

        var orders = await ordersQuery
            .Include(o => o.Creator)
            .Include(o => o.Payments)
            .ToListAsync();

        // Revenue calculations
        var totalRevenue = orders
            .Where(o => o.Status == OrderStatus.Paid)
            .Sum(o => o.Total);

        var todayRevenue = orders
            .Where(o => o.Status == OrderStatus.Paid && o.CreatedAt >= today)
            .Sum(o => o.Total);

        var thisWeekRevenue = orders
            .Where(o => o.Status == OrderStatus.Paid && o.CreatedAt >= weekStart)
            .Sum(o => o.Total);

        var thisMonthRevenue = orders
            .Where(o => o.Status == OrderStatus.Paid && o.CreatedAt >= monthStart)
            .Sum(o => o.Total);

        // Order counts
        var totalOrders = orders.Count;
        var todayOrders = orders.Count(o => o.CreatedAt >= today);
        var thisWeekOrders = orders.Count(o => o.CreatedAt >= weekStart);
        var thisMonthOrders = orders.Count(o => o.CreatedAt >= monthStart);
        var pendingOrders = orders.Count(o => o.Status == OrderStatus.Draft || o.Status == OrderStatus.Placed);
        var paidOrders = orders.Count(o => o.Status == OrderStatus.Paid);
        var cancelledOrders = orders.Count(o => o.Status == OrderStatus.Cancelled);

        // Payment method breakdown
        var allPayments = orders
            .Where(o => o.Status == OrderStatus.Paid)
            .SelectMany(o => o.Payments)
            .ToList();

        var paymentBreakdown = new PaymentMethodBreakdown
        {
            CashTotal = allPayments.Where(p => p.Method == PaymentMethod.Cash).Sum(p => p.Amount),
            CardTotal = allPayments.Where(p => p.Method == PaymentMethod.Card).Sum(p => p.Amount),
            GiftCardTotal = allPayments.Where(p => p.Method == PaymentMethod.GiftCard).Sum(p => p.Amount),
            CashCount = allPayments.Count(p => p.Method == PaymentMethod.Cash),
            CardCount = allPayments.Count(p => p.Method == PaymentMethod.Card),
            GiftCardCount = allPayments.Count(p => p.Method == PaymentMethod.GiftCard)
        };

        // Recent orders (last 10)
        var recentOrders = orders
            .OrderByDescending(o => o.CreatedAt)
            .Take(10)
            .Select(o => new OrderSummaryResponse
            {
                Id = o.Id,
                SpotId = o.SpotId,
                Total = o.Total,
                Status = o.Status.ToString(),
                CreatedAt = o.CreatedAt,
                CreatedByName = o.Creator?.Name ?? "Unknown"
            })
            .ToList();

        // Appointment statistics
        var appointmentsQuery = _context.Appointments
            .Where(a => a.BusinessId == businessId);

        if (startDate.HasValue)
        {
            appointmentsQuery = appointmentsQuery.Where(a => a.Date >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            appointmentsQuery = appointmentsQuery.Where(a => a.Date <= endDate.Value);
        }

        var appointments = await appointmentsQuery.ToListAsync();

        var totalAppointments = appointments.Count;
        var todayAppointments = appointments.Count(a => a.Date.Date == today);
        var upcomingAppointments = appointments.Count(a => a.Date >= now && a.Status != AppointmentStatus.Cancelled);
        var completedAppointments = appointments.Count(a => a.Status == AppointmentStatus.Completed);
        var cancelledAppointments = appointments.Count(a => a.Status == AppointmentStatus.Cancelled);

        return new DashboardResponse
        {
            TotalRevenue = totalRevenue,
            TodayRevenue = todayRevenue,
            ThisWeekRevenue = thisWeekRevenue,
            ThisMonthRevenue = thisMonthRevenue,
            TotalOrders = totalOrders,
            TodayOrders = todayOrders,
            ThisWeekOrders = thisWeekOrders,
            ThisMonthOrders = thisMonthOrders,
            PendingOrders = pendingOrders,
            PaidOrders = paidOrders,
            CancelledOrders = cancelledOrders,
            PaymentMethods = paymentBreakdown,
            RecentOrders = recentOrders,
            TotalAppointments = totalAppointments,
            TodayAppointments = todayAppointments,
            UpcomingAppointments = upcomingAppointments,
            CompletedAppointments = completedAppointments,
            CancelledAppointments = cancelledAppointments
        };
    }
}
