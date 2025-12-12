namespace backend.Models;

public enum OrderStatus
{
    Draft,
    Placed,
    Paid,
    Cancelled
}

public class Order
{
    public Order()
    {
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public int Id { get; set; }
    public int BusinessId { get; set; }
    public int SpotId { get; set; }
    public int CreatedBy { get; set; }
    public decimal SubTotal { get; set; }
    public decimal Discount { get; set; }
    public decimal Tax { get; set; }
    public decimal Total => SubTotal - Discount + Tax;
    public OrderStatus Status { get; set; } = OrderStatus.Draft;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public Business Business { get; set; } = null!;
    public User Creator { get; set; } = null!;
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public Appointment? Appointment { get; set; }
}
