using KnowMe.API.Shared.Extensions;

namespace KnowMe.API.Shared.Middleware;

/// <summary>
/// Simple username-based auth: reads the acting user's id from the X-User-Id header
/// and stashes it in the request context. Presence is not enforced here — endpoints
/// that require a user call <c>HttpContext.GetUserId()</c>, which returns 401 if absent.
/// </summary>
public class UserContextMiddleware
{
    private const string UserIdHeader = "X-User-Id";

    private readonly RequestDelegate _next;

    public UserContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(UserIdHeader, out var value)
            && Guid.TryParse(value, out var userId))
        {
            context.Items[HttpContextExtensions.UserIdItemKey] = userId;
        }

        await _next(context);
    }
}
