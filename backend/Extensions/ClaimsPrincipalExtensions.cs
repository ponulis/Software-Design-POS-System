using System.Security.Claims;

namespace backend.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static int? GetUserId(this ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
        return userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId) ? userId : null;
    }

    public static int? GetBusinessId(this ClaimsPrincipal user)
    {
        var businessIdClaim = user.FindFirst("business_id");
        return businessIdClaim != null && int.TryParse(businessIdClaim.Value, out var businessId) ? businessId : null;
    }

    public static string? GetUserRole(this ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.Role)?.Value;
    }

    public static string? GetUserName(this ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.Name)?.Value;
    }
}
