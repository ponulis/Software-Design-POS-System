namespace backend.Models;

public class GiftCard
{

    public int GiftCardId { get; set; }
    public int BusinessId { get; set; }
    public int PaymentId { get; set; }
    public decimal InitialAmount { get; set; }
    public decimal RemainingBalance { get; set; }
    public DateTime ExpiryDate { get; set; }
    
    public Payment Payment { get; set; } = default!;
}