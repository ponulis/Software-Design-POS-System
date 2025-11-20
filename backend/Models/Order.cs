namespace backend.Models;

public class Order
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }

    public string Customer { get; set; } = default!;
    public decimal Subtotal { get; set; }
    public decimal TaxTotal { get; set; }
    public decimal DiscountTotal { get; set; }
    public decimal ServiceCharge { get; set; }
    public decimal Total { get; set; }
    public decimal Tip { get; set; }

    public string Status { get; set; } = "Open"; // Open, Paid, Cancelled

    // Navigation
    public Employee Employee { get; set; } = default!;
    public List<OrderItem> OrderItems { get; set; } = new();
    public List<Discount> Discounts { get; set; } = new();
    public List<Payment> Payments { get; set; } = default!;
}