using System.Security.Claims;

namespace AdventureGuildAPI.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static int GetUserId(this ClaimsPrincipal user)
    {
        if (!int.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
        {
            throw new InvalidOperationException("The authenticated user does not have a valid ID claim.");
        }

        return userId;
    }
}
