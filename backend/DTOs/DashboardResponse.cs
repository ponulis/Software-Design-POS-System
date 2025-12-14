namespace backend.DTOs;

public class DashboardResponse
{
    // Revenue Statistics
    public decimal TotalRevenue { get; set; }
    public decimal TodayRevenue { get; set; }
    public decimal ThisWeekRevenue { get; set; }
    public decimal ThisMonthRevenue { get; set; }
    
    // Order Statistics
    public int TotalOrders { get; set; }
    public int TodayOrders { get; set; }
    public int ThisWeekOrders { get; set; }
    public int ThisMonthOrders { get; set; }
    public int PendingOrders { get; set; }
    public int PaidOrders { get; set; }
    public int CancelledOrders { get; set; }
    
    // Payment Method Breakdown
    public PaymentMethodBreakdown PaymentMethods { get; set; } = new();
    
    // Recent Orders
    public List<OrderSummaryResponse> RecentOrders { get; set; } = new();
    
    // Appointment Statistics
    public int TotalAppointments { get; set; }
    public int TodayAppointments { get; set; }
    public int UpcomingAppointments { get; set; }
    public int CompletedAppointments { get; set; }
    public int CancelledAppointments { get; set; }
}

public class PaymentMethodBreakdown
{
    public decimal CashTotal { get; set; }
    public decimal CardTotal { get; set; }
    public decimal GiftCardTotal { get; set; }
    public int CashCount { get; set; }
    public int CardCount { get; set; }
    public int GiftCardCount { get; set; }
}

public class OrderSummaryResponse
{
    public int Id { get; set; }
    public int SpotId { get; set; }
    public decimal Total { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
}
