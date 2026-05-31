using KnowMe.API.Domain.Validation;

namespace KnowMe.API.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string UserName { get; private set; }
    public string? ProfileUrl { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public static Result<User> Create(string username) => Create(Guid.NewGuid(), username);

    /// <summary>
    /// Creates a user whose identity is supplied by an external identity provider
    /// (e.g. the Cognito subject claim) rather than generated here.
    /// </summary>
    public static Result<User> Register(Guid id, string username) => Create(id, username);

    private static Result<User> Create(Guid id, string username)
    {
        var errors = new List<ValidationError>();

        if (username.Length > 100)
        {
            errors.Add(new ValidationError
            {
                Message = "Username cannot be longer than 100 characters"
            });
        }

        if (errors.Count != 0)
        {
            return Result<User>.Failure(errors);
        }

        var user = new User
        {
            Id = id,
            UserName = username
        };

        return Result<User>.Success(user);
    }

    /// <summary>
    /// Reconstructs a user from persisted state without running creation validation.
    /// For use by the persistence layer only.
    /// </summary>
    internal static User Rehydrate(Guid id, string userName, string? profileUrl, DateTimeOffset createdAt) =>
        new()
        {
            Id = id,
            UserName = userName,
            ProfileUrl = profileUrl,
            CreatedAt = createdAt
        };
}
