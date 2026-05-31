using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.AspNetCoreServer;
using KnowMe.API.Shared.Extensions;

namespace KnowMe.API.Shared.Middleware;

/// <summary>
/// Production auth: reads the verified user id (Cognito subject) that the Lambda authorizer
/// placed in the API Gateway request context. The client cannot supply or spoof it — requests
/// without a valid JWT never reach the Lambda (API Gateway rejects them at the authorizer).
/// </summary>
public class AuthorizerContextMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuthorizerContextMiddleware> _logger;

    public AuthorizerContextMiddleware(RequestDelegate next, ILogger<AuthorizerContextMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // /health is a public route (no authorizer), so there is nothing to read.
        if (context.Request.Path.StartsWithSegments("/health"))
        {
            await _next(context);
            return;
        }

        var lambdaRequest = context.Items[AbstractAspNetCoreFunction.LAMBDA_REQUEST_OBJECT]
            as APIGatewayHttpApiV2ProxyRequest;

        var sub = lambdaRequest?.RequestContext?.Authorizer?.Lambda is { } claims
                  && claims.TryGetValue("userId", out var value)
            ? value?.ToString()
            : null;

        if (!Guid.TryParse(sub, out var userId))
        {
            _logger.LogWarning("[AuthorizerContext] No valid userId in the authorizer context.");
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        context.Items[HttpContextExtensions.UserIdItemKey] = userId;
        await _next(context);
    }
}
