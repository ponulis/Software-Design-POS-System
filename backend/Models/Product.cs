namespace backend.Models;

public class Product
{
    public int Id { get; set; }
    public int BusinessId { get; set; }
    public int TaxId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public decimal TaxRate { get; set; }
    
    
    public Tax Tax { get; set; }
    public Business Business { get; set; }
    public List<OrderItem> OrderItems { get; set; }
    
}