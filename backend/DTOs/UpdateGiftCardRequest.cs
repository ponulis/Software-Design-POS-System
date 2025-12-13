namespace backend.DTOs;

public class UpdateGiftCardRequest
{
    public decimal? Balance { get; set; }
    public bool? IsActive { get; set; }
    public DateTime? ExpiryDate { get; set; }
}
