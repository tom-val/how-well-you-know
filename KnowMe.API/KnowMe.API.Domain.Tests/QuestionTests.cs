using FluentAssertions;
using KnowMe.API.Domain.Entities;

namespace KnowMe.API.Domain.Tests;

public class QuestionTests
{
    [Fact]
    public void Create_WhenValidParametersGiven_CreatesQuestion()
    {
        var user = User.Create("player1").Value;
        var gameResult = Game.Create("test game", user).Value;

        var questionVariants = new Dictionary<char, string>()
        {
            { 'A', "Yes" },
            { 'B', "No" }
        };

        var questionResult = Question.Create("Is this the real life?", false, questionVariants, user, gameResult);
        questionResult.IsSuccess.Should().BeTrue();
        questionResult.Value.Text.Should().Be("Is this the real life?");
    }

    [Fact]
    public void Create_WhenTooLongQuestionTextGiven_ReturnsError()
    {
        var user = User.Create("player1").Value;
        var gameResult = Game.Create("test game", user).Value;

        var questionVariants = new Dictionary<char, string>()
        {
            { 'A', "Yes" },
            { 'B', "No" }
        };

        var questionResult = Question.Create("Is this the real life?Is this the real life?Is this the real life?Is this the real life?Is this the real life?", false, questionVariants, user, gameResult);
        questionResult.IsSuccess.Should().BeFalse();
        questionResult.Errors!.First().Message.Should().Be("Question text cannot be longer than 100 characters");
    }

    [Fact]
    public void Create_WhenTooQuestionVariantTextGiven_ReturnsError()
    {
        var user = User.Create("player1").Value;
        var gameResult = Game.Create("test game", user).Value;

        var questionVariants = new Dictionary<char, string>
        {
            { 'A', "YesYesYesYesYesYesYesYesYesYesYesYesYesYesYesYesYesYesYesYesYesYesYesYesYesYesYesYesYesYesYesYesYesYesYesYesYes" },
            { 'B', "No" }
        };

        var questionResult = Question.Create("Is this the real life?", false, questionVariants, user, gameResult);
        questionResult.IsSuccess.Should().BeFalse();
        questionResult.Errors!.First().Message.Should().Be("Answer text cannot be longer than 100 characters");
    }

    [Fact]
    public void Create_WhenNotEnoughQuestionVariantsGiven_ReturnsError()
    {
        var user = User.Create("player1").Value;
        var gameResult = Game.Create("test game", user).Value;

        var questionVariants = new Dictionary<char, string>()
        {
            { 'A', "Yes" }
        };

        var questionResult = Question.Create("Is this the real life?", false, questionVariants, user, gameResult);
        questionResult.IsSuccess.Should().BeFalse();
        questionResult.Errors!.First().Message.Should().Be("More than one possible question answer must be added");
    }

    [Fact]
    public void Create_WhenTooMuchQuestionVariantsGiven_ReturnsError()
    {
        var user = User.Create("player1").Value;
        var gameResult = Game.Create("test game", user).Value;

        var questionVariants = new Dictionary<char, string>()
        {
            { 'A', "Yes" },
            { 'B', "Yes" },
            { 'C', "Yes" },
            { 'D', "Yes" },
            { 'E', "Yes" },
            { 'F', "Yes" },
            { 'G', "Yes" },
            { 'H', "Yes" },
            { 'I', "Yes" },
            { 'J', "Yes" },
            { 'K', "Yes" },
            { 'L', "Yes" },
            { 'M', "Yes" },
            { 'N', "Yes" },
            { 'O', "Yes" },
            { 'P', "Yes" },
            { 'Q', "Yes" },
            { 'R', "Yes" },
            { 'S', "Yes" },
            { 'T', "Yes" },
            { 'X', "Yes" },
            { 'Y', "Yes" }
        };

        var questionResult = Question.Create("Is this the real life?", false, questionVariants, user, gameResult);
        questionResult.IsSuccess.Should().BeFalse();
        questionResult.Errors!.First().Message.Should().Be("No more than twenty possible question answer must be added");
    }
}
