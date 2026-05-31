using KnowMe.API.Domain.Entities;

namespace KnowMe.API.Features.Users;

public record CreateUserRequest(string UserName);

public record UserResponse(Guid Id, string UserName, string? ProfileUrl)
{
    public static UserResponse From(User user) => new(user.Id, user.UserName, user.ProfileUrl);
}
