using FluentAssertions;
using KnowMe.API.Domain.Entities;

namespace KnowMe.API.Domain.Tests;

public class UserTests
{
    [Fact]
    public void Create_WhenValidParametersGiven_CreatesUser()
    {
        var userResult = User.Create("testUser");

        userResult.IsSuccess.Should().BeTrue();
        userResult.Value.UserName.Should().Be("testUser");
    }

    [Fact]
    public void Create_WhenMoreThan100CharactersUsername_ReturnsError()
    {
        var userResult = User.Create("testUsertestUsertestUsertestUsertestUsertestUsertestUsertestUsertestUsertestUsertestUsertestUsertestUsertestUsertestUsertestUser");

        userResult.IsSuccess.Should().BeFalse();
        userResult.Errors!.Count.Should().Be(1);
        userResult.Errors[0].Message.Should().Be("Username cannot be longer than 100 characters");
    }
}
