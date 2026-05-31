using FluentAssertions;
using KnowMe.API.Domain.Entities;
using KnowMe.API.Domain.Enums;

namespace KnowMe.API.Domain.Tests;

public class GameFlowTests
{
    [Fact]
    public void TwoPlayersShouldBeAbleToFinishGame()
    {
        var player1 = User.Create("player1");
        var player2 = User.Create("player2");

        //Create game
        var game = Game.Create("Test game", player1.Value).Value;

        //Player 2 joins
        game.AddPlayer(player2.Value);

        //Question 1
        var question1Variants = new Dictionary<char, string>()
        {
            { 'A', "Blue" },
            { 'B', "Green" },
            { 'C', "Yellow" }
        };
        var question1 = Question.Create("Favourite color?", false, question1Variants, player1.Value, game);
        game.AddQuestion(question1.Value);

        //Question 2
        var question2Variants = new Dictionary<char, string>()
        {
            { 'A', "Vilnius" },
            { 'B', "Kaunas" },
            { 'C', "Klaipeda" }
        };
        var question2 = Question.Create("Best place to be?", true, question2Variants, player2.Value, game);
        game.AddQuestion(question2.Value);

        //Question 3
        var question3Variants = new Dictionary<char, string>()
        {
            { 'A', "Miškas" },
            { 'B', "Jūra" }
        };
        var question3 = Question.Create("Miškas ar jūra?", true, question3Variants, player2.Value, game);
        game.AddQuestion(question3.Value);

        //Start game
        game.StartGame();
    }

    [Fact]
    public void AnsweringCurrentQuestion_MovesItToReviewWithoutAdvancing()
    {
        var game = TestGameBuilder.StartedTwoPlayerGame(out var player1, out var player2);
        var firstQuestionId = game.CurrentQuestionId;

        game.AnswerCurrentQuestion(player1, player2);

        //Game pauses on a per-question review step instead of auto-advancing
        game.Status.Should().Be(GameStatus.Started);
        game.CurrentQuestionPhase.Should().Be(QuestionPhase.Review);
        game.CurrentQuestionId.Should().Be(firstQuestionId);
        game.CurrentQuestion().Answered.Should().BeTrue();
    }

    [Fact]
    public void AdvanceToNextQuestion_WhenInReview_MovesToNextQuestionInAnsweringPhase()
    {
        var game = TestGameBuilder.StartedTwoPlayerGame(out var player1, out var player2);
        var firstQuestionId = game.CurrentQuestionId;

        game.AnswerCurrentQuestion(player1, player2);
        var result = game.AdvanceToNextQuestion();

        result.IsSuccess.Should().BeTrue();
        game.Status.Should().Be(GameStatus.Started);
        game.CurrentQuestionPhase.Should().Be(QuestionPhase.Answering);
        game.CurrentQuestionId.Should().NotBe(firstQuestionId);
    }

    [Fact]
    public void AdvanceToNextQuestion_WhenNotInReview_ReturnsError()
    {
        var game = TestGameBuilder.StartedTwoPlayerGame(out _, out _);

        var result = game.AdvanceToNextQuestion();

        result.IsSuccess.Should().BeFalse();
        result.Errors!.First().Message.Should().Be("Cannot advance until the current question is in review");
    }

    [Fact]
    public void AdvanceToNextQuestion_AfterLastQuestion_EndsGame()
    {
        var game = TestGameBuilder.StartedTwoPlayerGame(out var player1, out var player2);

        //Question 1
        game.AnswerCurrentQuestion(player1, player2);
        game.AdvanceToNextQuestion();

        //Question 2 (last)
        game.AnswerCurrentQuestion(player1, player2);
        var result = game.AdvanceToNextQuestion();

        result.IsSuccess.Should().BeTrue();
        game.Status.Should().Be(GameStatus.Ended);
    }
}
