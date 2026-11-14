using System.Security.Claims;

namespace api.Extensions;

public static class ClaimsExtensions
{
    public static string GetUserId(this ClaimsPrincipal user) =>
        user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("The token has no user id claim.");

    public static string GetUserName(this ClaimsPrincipal user) =>
        user.Identity?.Name
            ?? throw new InvalidOperationException("The token has no user name claim.");
}
