using FluentAssertions;
using KnowMe.API.Domain.Entities;
using KnowMe.API.Domain.Enums;

namespace KnowMe.API.Domain.Tests;

public class GameTests
{
    [Fact]
    public void Create_WhenValidParametersGiven_CreatesGame()
    {
        var user = User.Create("player1");
        var gameResult = Game.Create("test game", user.Value);

        gameResult.IsSuccess.Should().BeTrue();
        gameResult.Value.Name.Should().Be("test game");
        gameResult.Value.Players[0].Id.Should().Be(user.Value.Id);
        gameResult.Value.Status.Should().Be(GameStatus.Created);
    }

    [Fact]
    public void Create_WhenMoreThan100CharactersGameName_ReturnsError()
    {
        var user = User.Create("player1");
        var gameResult = Game.Create("TestTestTestTestTestTestTestTestTestTestTestTestTestTestTestTestTestTestTestTestTestTestTestTestTestTest", user.Value);

        gameResult.IsSuccess.Should().BeFalse();
        gameResult.Errors!.Count.Should().Be(1);
        gameResult.Errors[0].Message.Should().Be("Name cannot be longer than 100 characters");
    }

    [Fact]
    public void StartGame_WhenNotEnoughPlayers_ReturnsError()
    {
        var user = User.Create("player1");
        var gameResult = Game.Create("test game", user.Value);
        var game = gameResult.Value;

        var gameStartResult = game.StartGame();
        gameStartResult.IsSuccess.Should().BeFalse();
        gameStartResult.Errors![0].Message.Should().Be("Cannot start game with only one player");
    }

    [Fact]
    public void StartGame_WhenNotEnoughQuestions_ReturnsError()
    {
        var user = User.Create("player1");
        var gameResult = Game.Create("test game", user.Value);
        var game = gameResult.Value;

        game.AddPlayer(User.Create("test").Value);

        var question1Variants = new Dictionary<char, string>()
        {
            { 'A', "Blue" },
            { 'B', "Green" },
            { 'C', "Yellow" }
        };
        var question1 = Question.Create("Favourite color?", false, question1Variants, user.Value, game);
        game.AddQuestion(question1.Value);

        var gameStartResult = game.StartGame();
        gameStartResult.IsSuccess.Should().BeFalse();
        gameStartResult.Errors![0].Message.Should().Be("At least two questions required to start the game");
    }

    [Fact]
    public void StartGame_WhenEnoughPlayersAndQuestions_ReturnsError()
    {
        var user = User.Create("player1");
        var gameResult = Game.Create("test game", user.Value);
        var game = gameResult.Value;

        game.AddPlayer(User.Create("test").Value);

        var question1Variants = new Dictionary<char, string>()
        {
            { 'A', "Blue" },
            { 'B', "Green" },
            { 'C', "Yellow" }
        };
        var question1 = Question.Create("Favourite color?", false, question1Variants, user.Value, game);
        game.AddQuestion(question1.Value);

        //Question 2
         var question2Variants = new Dictionary<char, string>()
         {
             { 'A', "Vilnius" },
             { 'B', "Kaunas" },
             { 'C', "Klaipeda" }
         };
         var question2 = Question.Create("Best place to be?", true, question2Variants, user.Value, game);
         game.AddQuestion(question2.Value);

        var gameStartResult = game.StartGame();
        gameStartResult.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void StartGame_WhenAlreadyStarted_ReturnsError()
    {
        var game = TestGameBuilder.StartedTwoPlayerGame(out _, out _);

        var result = game.StartGame();

        result.IsSuccess.Should().BeFalse();
        result.Errors!.Should().Contain(e => e.Message == "Game has already been started");
    }

    [Fact]
    public void AddPlayer_WhenDuplicate_ReturnsError()
    {
        var user = User.Create("player1").Value;
        var game = Game.Create("test game", user).Value;

        var result = game.AddPlayer(user);

        result.IsSuccess.Should().BeFalse();
        result.Errors!.First().Message.Should().Be("Cannot add duplicate player");
    }

    [Fact]
    public void AddPlayer_AfterGameStarted_ReturnsError()
    {
        var game = TestGameBuilder.StartedTwoPlayerGame(out _, out _);

        var result = game.AddPlayer(User.Create("late joiner").Value);

        result.IsSuccess.Should().BeFalse();
        result.Errors!.First().Message.Should().Be("Cannot add players after the game has started");
    }

    [Fact]
    public void AddQuestion_AfterGameStarted_ReturnsError()
    {
        var game = TestGameBuilder.StartedTwoPlayerGame(out var player1, out _);

        var variants = new Dictionary<char, string> { { 'A', "Yes" }, { 'B', "No" } };
        var question = Question.Create("Late question?", false, variants, player1, game).Value;

        var result = game.AddQuestion(question);

        result.IsSuccess.Should().BeFalse();
        result.Errors!.First().Message.Should().Be("Cannot add questions after the game has started");
    }
}
