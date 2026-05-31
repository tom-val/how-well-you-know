using FluentValidation;
using KnowMe.API.Domain.Entities;
using KnowMe.API.Persistence.Repositories;
using KnowMe.API.Shared;

namespace KnowMe.API.Features.Users;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/v1/users");

        group.MapPost("/", CreateOrGetUser);
        group.MapGet("/{id:guid}", GetUser);
    }

    // Create-or-return by username, matching the casual party-game flow (no passwords).
    private static async Task<IResult> CreateOrGetUser(
        CreateUserRequest request,
        IValidator<CreateUserRequest> validator,
        IUserRepository users,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return Results.ValidationProblem(validation.ToDictionary());

        var existing = await users.GetByUserNameAsync(request.UserName, cancellationToken);
        if (existing is not null)
            return Results.Ok(UserResponse.From(existing));

        var result = User.Create(request.UserName);
        if (!result.IsSuccess)
            return ApiResults.DomainValidationProblem(result.Errors!);

        await users.SaveAsync(result.Value, cancellationToken);
        return Results.Created($"/v1/users/{result.Value.Id}", UserResponse.From(result.Value));
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
