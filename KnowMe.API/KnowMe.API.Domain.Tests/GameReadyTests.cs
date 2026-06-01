using FluentAssertions;
using KnowMe.API.Domain.Entities;
using KnowMe.API.Domain.Enums;

namespace KnowMe.API.Domain.Tests;

public class GameReadyTests
{
    // A lobby-stage two-player game with two questions — meets the start minimums but not started.
    private static Game CreatedGame(out User player1, out User player2, int questions = 2)
    {
        player1 = User.Create("player1").Value;
        player2 = User.Create("player2").Value;

        var game = Game.Create("test game", player1).Value;
        game.AddPlayer(player2);

        for (var i = 0; i < questions; i++)
        {
            var variants = new Dictionary<char, string> { { 'A', "Yes" }, { 'B', "No" } };
            game.AddQuestion(Question.Create($"Question {i}?", false, variants, player1, game).Value);
        }

        return game;
    }

    [Fact]
    public void SetReady_WhenNotEveryoneReady_StaysInLobby()
    {
        var game = CreatedGame(out var p1, out _);

        game.SetReady(p1, true).IsSuccess.Should().BeTrue();

        game.Status.Should().Be(GameStatus.Created);
        game.ReadyUserIds.Should().ContainSingle().Which.Should().Be(p1.Id);
    }

    [Fact]
    public void SetReady_WhenEveryoneReady_StartsGame()
    {
        var game = CreatedGame(out var p1, out var p2);

        game.SetReady(p1, true);
        game.SetReady(p2, true);

        game.Status.Should().Be(GameStatus.Started);
        game.CurrentQuestionPhase.Should().Be(QuestionPhase.Answering);
    }

    [Fact]
    public void SetReady_WhenUnreadied_DoesNotStart()
    {
        var game = CreatedGame(out var p1, out var p2);

        game.SetReady(p1, true);
        game.SetReady(p1, false); // changed mind
        game.SetReady(p2, true);

        game.Status.Should().Be(GameStatus.Created);
        game.ReadyUserIds.Should().ContainSingle().Which.Should().Be(p2.Id);
    }

    [Fact]
    public void SetReady_WhenMinimumsNotMet_DoesNotStart()
    {
        var game = CreatedGame(out var p1, out var p2, questions: 1);

        game.SetReady(p1, true);
        game.SetReady(p2, true);

        game.Status.Should().Be(GameStatus.Created);
    }

    [Fact]
    public void SetReady_WhenUserIsNotAPlayer_ReturnsError()
    {
        var game = CreatedGame(out _, out _);
        var outsider = User.Create("outsider").Value;

        var result = game.SetReady(outsider, true);

        result.IsSuccess.Should().BeFalse();
        result.Errors![0].Message.Should().Be("User is not a player in this game");
    }

    [Fact]
    public void SetReady_WhenGameAlreadyStarted_ReturnsError()
    {
        var game = CreatedGame(out var p1, out var p2);
        game.SetReady(p1, true);
        game.SetReady(p2, true); // auto-starts

        var result = game.SetReady(p1, false);

        result.IsSuccess.Should().BeFalse();
        result.Errors![0].Message.Should().Be("Game has already been started");
    }

    [Fact]
    public void AddQuestion_WhenEveryoneReadyAndReachesMinimum_AutoStarts()
    {
        var game = CreatedGame(out var p1, out var p2, questions: 1);
        game.SetReady(p1, true);
        game.SetReady(p2, true); // can't start yet — only one question

        game.Status.Should().Be(GameStatus.Created);

        var variants = new Dictionary<char, string> { { 'A', "Yes" }, { 'B', "No" } };
        game.AddQuestion(Question.Create("Another?", false, variants, p1, game).Value);

        game.Status.Should().Be(GameStatus.Started);
    }
}
