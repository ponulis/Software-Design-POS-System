using backend.Data;
using backend.DTOs;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class GiftCardService
{
    private readonly ApplicationDbContext _context;

    public GiftCardService(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Validate gift card and get balance
    /// </summary>
    public async Task<GiftCardResponse?> ValidateAndGetBalanceAsync(string code, int businessId)
    {
        var giftCard = await _context.GiftCards
            .Where(gc => gc.Code == code && gc.BusinessId == businessId)
            .FirstOrDefaultAsync();

        if (giftCard == null)
        {
            return null;
        }

        // Validate gift card is active
        if (!giftCard.IsActive)
        {
            throw new InvalidOperationException("Gift card is not active");
        }

        // Validate gift card has not expired
        if (giftCard.ExpiryDate.HasValue && giftCard.ExpiryDate.Value < DateTime.UtcNow)
        {
            throw new InvalidOperationException($"Gift card expired on {giftCard.ExpiryDate.Value:yyyy-MM-dd}");
        }

        // Validate gift card has balance
        if (giftCard.Balance <= 0)
        {
            throw new InvalidOperationException("Gift card has no remaining balance");
        }

        return MapToGiftCardResponse(giftCard);
    }

    /// <summary>
    /// Create/issue a new gift card
    /// </summary>
    public async Task<GiftCardResponse> CreateGiftCardAsync(CreateGiftCardRequest request, int businessId)
    {
        // Validate original amount
        if (request.OriginalAmount <= 0)
        {
            throw new InvalidOperationException("Original amount must be greater than zero");
        }

        // Generate code if not provided
        string code = string.IsNullOrWhiteSpace(request.Code)
            ? GenerateGiftCardCode()
            : request.Code.ToUpper().Trim();

        // Check if code already exists
        var existingGiftCard = await _context.GiftCards
            .Where(gc => gc.Code == code && gc.BusinessId == businessId)
            .FirstOrDefaultAsync();

        if (existingGiftCard != null)
        {
            throw new InvalidOperationException($"Gift card with code '{code}' already exists");
        }

        // Validate expiry date is in the future if provided
        if (request.ExpiryDate.HasValue && request.ExpiryDate.Value < DateTime.UtcNow)
        {
            throw new InvalidOperationException("Expiry date must be in the future");
        }

        var giftCard = new GiftCard
        {
            BusinessId = businessId,
            Code = code,
            Balance = request.OriginalAmount,
            OriginalAmount = request.OriginalAmount,
            IssuedDate = DateTime.UtcNow,
            ExpiryDate = request.ExpiryDate,
            IsActive = true
        };

        _context.GiftCards.Add(giftCard);
        await _context.SaveChangesAsync();

        return MapToGiftCardResponse(giftCard);
    }

    /// <summary>
    /// Update gift card balance (typically on redemption)
    /// </summary>
    public async Task<GiftCardResponse?> UpdateGiftCardAsync(string code, UpdateGiftCardRequest request, int businessId)
    {
        var giftCard = await _context.GiftCards
            .Where(gc => gc.Code == code && gc.BusinessId == businessId)
            .FirstOrDefaultAsync();

        if (giftCard == null)
        {
            return null;
        }

        // Update balance if provided
        if (request.Balance.HasValue)
        {
            if (request.Balance.Value < 0)
            {
                throw new InvalidOperationException("Balance cannot be negative");
            }

            giftCard.Balance = request.Balance.Value;
        }

        // Update IsActive if provided
        if (request.IsActive.HasValue)
        {
            giftCard.IsActive = request.IsActive.Value;
        }

        // Update ExpiryDate if provided
        if (request.ExpiryDate.HasValue)
        {
            if (request.ExpiryDate.Value < DateTime.UtcNow)
            {
                throw new InvalidOperationException("Expiry date must be in the future");
            }

            giftCard.ExpiryDate = request.ExpiryDate.Value;
        }

        await _context.SaveChangesAsync();

        return MapToGiftCardResponse(giftCard);
    }

    /// <summary>
    /// Credit amount back to gift card balance (for refunds)
    /// </summary>
    public async Task<GiftCardResponse> CreditGiftCardAsync(string code, decimal amount, int businessId)
    {
        var giftCard = await _context.GiftCards
            .Where(gc => gc.Code == code && gc.BusinessId == businessId)
            .FirstOrDefaultAsync();

        if (giftCard == null)
        {
            throw new InvalidOperationException("Gift card not found");
        }

        // Validate amount is positive
        if (amount <= 0)
        {
            throw new InvalidOperationException("Credit amount must be greater than zero");
        }

        // Credit balance back
        giftCard.Balance += amount;

        // Ensure balance doesn't exceed original amount (if there's a cap)
        // Note: This is optional - some systems allow balance to exceed original amount
        // if multiple credits are applied

        await _context.SaveChangesAsync();

        return MapToGiftCardResponse(giftCard);
    }

    /// <summary>
    /// Deduct amount from gift card balance
    /// </summary>
    public async Task<GiftCardResponse> DeductBalanceAsync(string code, decimal amount, int businessId)
    {
        var giftCard = await _context.GiftCards
            .Where(gc => gc.Code == code && gc.BusinessId == businessId)
            .FirstOrDefaultAsync();

        if (giftCard == null)
        {
            throw new InvalidOperationException("Gift card not found");
        }

        // Validate gift card is active
        if (!giftCard.IsActive)
        {
            throw new InvalidOperationException("Gift card is not active");
        }

        // Validate gift card has not expired
        if (giftCard.ExpiryDate.HasValue && giftCard.ExpiryDate.Value < DateTime.UtcNow)
        {
            throw new InvalidOperationException($"Gift card expired on {giftCard.ExpiryDate.Value:yyyy-MM-dd}");
        }

        // Validate amount is positive
        if (amount <= 0)
        {
            throw new InvalidOperationException("Deduction amount must be greater than zero");
        }

        // Validate sufficient balance
        if (giftCard.Balance < amount)
        {
            throw new InvalidOperationException($"Insufficient balance. Available: {giftCard.Balance:C}, Required: {amount:C}");
        }

        // Deduct balance
        giftCard.Balance -= amount;

        await _context.SaveChangesAsync();

        return MapToGiftCardResponse(giftCard);
    }

    /// <summary>
    /// Get all gift cards for a business
    /// </summary>
    public async Task<List<GiftCardResponse>> GetAllGiftCardsAsync(int businessId, bool? activeOnly = null)
    {
        var query = _context.GiftCards
            .Where(gc => gc.BusinessId == businessId);

        if (activeOnly == true)
        {
            query = query.Where(gc => gc.IsActive);
        }

        var giftCards = await query
            .OrderByDescending(gc => gc.IssuedDate)
            .ToListAsync();

        return giftCards.Select(MapToGiftCardResponse).ToList();
    }

    /// <summary>
    /// Generate a unique gift card code
    /// </summary>
    private string GenerateGiftCardCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        var code = new string(Enumerable.Repeat(chars, 12)
            .Select(s => s[random.Next(s.Length)]).ToArray());
        
        // Format as XXXX-XXXX-XXXX
        return $"{code.Substring(0, 4)}-{code.Substring(4, 4)}-{code.Substring(8, 4)}";
    }

    private GiftCardResponse MapToGiftCardResponse(GiftCard giftCard)
    {
        return new GiftCardResponse
        {
            Id = giftCard.Id,
            Code = giftCard.Code,
            Balance = giftCard.Balance,
            OriginalAmount = giftCard.OriginalAmount,
            IssuedDate = giftCard.IssuedDate,
            ExpiryDate = giftCard.ExpiryDate,
            IsActive = giftCard.IsActive
        };
    }
}
