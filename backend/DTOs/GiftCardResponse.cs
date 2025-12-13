namespace backend.DTOs;

public class GiftCardResponse
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public decimal OriginalAmount { get; set; }
    public DateTime IssuedDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public bool IsActive { get; set; }
}
