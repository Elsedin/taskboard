using System.Security.Claims;

namespace TaskBoard.Api.Helpers;

public static class ClaimsPrincipalExtensions
{
    public static int GetUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (value is null || !int.TryParse(value, out var userId))
            throw new InvalidOperationException("User id claim is missing.");

        return userId;
    }
}