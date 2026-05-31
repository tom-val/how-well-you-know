using FluentAssertions;
using KnowMe.API.Domain.Entities;
using KnowMe.API.Domain.Enums;

namespace KnowMe.API.Domain.Tests;

public class GameRecordingTests
{
    [Fact]
    public void RecordChoice_WhenGameNotStarted_ReturnsError()
    {
        var player1 = User.Create("player1").Value;
        var game = Game.Create("test game", player1).Value;

        var result = game.RecordChoice(player1, []);

        result.IsSuccess.Should().BeFalse();
        result.Errors!.First().Message.Should().Be("Game is not in progress");
    }

    [Fact]
    public void RecordChoice_WhenValid_RecordsChoice()
    {
        var game = TestGameBuilder.StartedTwoPlayerGame(out var player1, out _);
        var question = game.CurrentQuestion();

        var result = game.RecordChoice(player1, [question.Variant('A')]);

        result.IsSuccess.Should().BeTrue();
        question.UserChoices.Should().ContainSingle(c => c.UserId == player1.Id);
    }

    [Fact]
    public void RecordChoice_WhenUserNotPlayer_ReturnsError()
    {
        var game = TestGameBuilder.StartedTwoPlayerGame(out _, out _);
        var outsider = User.Create("outsider").Value;
        var question = game.CurrentQuestion();

        var result = game.RecordChoice(outsider, [question.Variant('A')]);

        result.IsSuccess.Should().BeFalse();
        result.Errors!.First().Message.Should().Be("User is not a player in this game");
    }

    [Fact]
    public void RecordChoice_WhenCurrentQuestionInReview_ReturnsError()
    {
        var game = TestGameBuilder.StartedTwoPlayerGame(out var player1, out var player2);
        game.AnswerCurrentQuestion(player1, player2);

        game.CurrentQuestionPhase.Should().Be(QuestionPhase.Review);

        var result = game.RecordChoice(player1, [game.CurrentQuestion().Variant('B')]);

        result.IsSuccess.Should().BeFalse();
        result.Errors!.First().Message.Should().Be("Current question is not accepting answers");
    }

    [Fact]
    public void RecordGuess_WhenGuessingOwnAnswer_ReturnsError()
    {
        var game = TestGameBuilder.StartedTwoPlayerGame(out var player1, out _);
        var question = game.CurrentQuestion();

        var result = game.RecordGuess(player1, player1, [question.Variant('A')]);

        result.IsSuccess.Should().BeFalse();
        result.Errors!.Should().Contain(e => e.Message == "User cannot guess their own answer");
    }

    [Fact]
    public void RecordGuess_WhenGuessingUserNotPlayer_ReturnsError()
    {
        var game = TestGameBuilder.StartedTwoPlayerGame(out _, out var player2);
        var outsider = User.Create("outsider").Value;
        var question = game.CurrentQuestion();

        var result = game.RecordGuess(outsider, player2, [question.Variant('A')]);

        result.IsSuccess.Should().BeFalse();
        result.Errors!.Should().Contain(e => e.Message == "Both users must be players in this game");
    }

    [Fact]
    public void RecordGuess_WhenValid_RecordsGuess()
    {
        var game = TestGameBuilder.StartedTwoPlayerGame(out var player1, out var player2);
        var question = game.CurrentQuestion();

        var result = game.RecordGuess(player1, player2, [question.Variant('A')]);

        result.IsSuccess.Should().BeTrue();
        question.UserGuesses.Should().ContainSingle(g => g.GuessingUserId == player1.Id && g.ChoiceUserId == player2.Id);
    }
}
