namespace KnowMe.API.Shared.Extensions;

public static class HttpContextExtensions
{
    public const string UserIdItemKey = "UserId";

    /// <summary>
    /// The acting user's id, populated from the X-User-Id header by
    /// <c>UserContextMiddleware</c>. Throws when absent so the endpoint returns 401.
    /// </summary>
    public static Guid GetUserId(this HttpContext context)
    {
        return context.Items[UserIdItemKey] is Guid id
            ? id
            : throw new UnauthorizedAccessException("Missing or invalid X-User-Id header.");
    }

    public static Guid? TryGetUserId(this HttpContext context)
    {
        return context.Items[UserIdItemKey] as Guid?;
    }
}
