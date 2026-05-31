using FluentValidation;
using KnowMe.API.Domain.Entities;
using KnowMe.API.Persistence.Repositories;
using KnowMe.API.Shared;
using KnowMe.API.Shared.Extensions;

namespace KnowMe.API.Features.Users;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/v1/users");

        group.MapPost("/", RegisterCurrentUser);
        group.MapGet("/me", GetCurrentUser);
        group.MapGet("/{id:guid}", GetUser);
    }

    // Registers the authenticated caller as a player, keyed by their verified identity
    // (the token subject). Idempotent — returns the existing profile if already registered.
    private static async Task<IResult> RegisterCurrentUser(
        CreateUserRequest request,
        IValidator<CreateUserRequest> validator,
        IUserRepository users,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return Results.ValidationProblem(validation.ToDictionary());

        var userId = context.GetUserId();

        var existing = await users.GetAsync(userId, cancellationToken);
        if (existing is not null)
            return Results.Ok(UserResponse.From(existing));

        var result = User.Register(userId, request.UserName);
        if (!result.IsSuccess)
            return ApiResults.DomainValidationProblem(result.Errors!);

        await users.SaveAsync(result.Value, cancellationToken);
        return Results.Created($"/v1/users/{result.Value.Id}", UserResponse.From(result.Value));
    }

    private static async Task<IResult> GetCurrentUser(
        IUserRepository users,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var user = await users.GetAsync(context.GetUserId(), cancellationToken);
        return user is null
            ? Results.NotFound(new { error = "User not registered." })
            : Results.Ok(UserResponse.From(user));
    }

    private static async Task<IResult> GetUser(
        Guid id,
        IUserRepository users,
        CancellationToken cancellationToken)
    {
        var user = await users.GetAsync(id, cancellationToken);
        return user is null
            ? Results.NotFound(new { error = "User not found." })
            : Results.Ok(UserResponse.From(user));
    }
}
