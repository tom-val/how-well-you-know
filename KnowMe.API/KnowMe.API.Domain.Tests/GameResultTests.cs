using FluentAssertions;
using KnowMe.API.Domain.Entities;

namespace KnowMe.API.Domain.Tests;

public class GameResultTests
{
    [Fact]
    public void GetResults_BeforeAnyQuestionAnswered_ReturnsZeroForAllPlayers()
    {
        var game = TestGameBuilder.StartedTwoPlayerGame(out _, out _);

        var result = game.GetResults();

        result.GameId.Should().Be(game.Id);
        result.Scores.Should().HaveCount(2);
        result.Scores.Should().OnlyContain(s => s.TotalScore == 0);
        result.Scores.Should().OnlyContain(s => s.Rank == 1);
    }

    [Fact]
    public void GetResults_MidGame_OnlyCountsAnsweredQuestions()
    {
        var game = TestGameBuilder.StartedTwoPlayerGame(out var player1, out var player2);

        //Answer the first question only (both guess correctly -> +1 each), leaving it in review
        game.AnswerCurrentQuestion(player1, player2);

        var result = game.GetResults();

        result.Scores.Should().OnlyContain(s => s.TotalScore == 1);
    }

    [Fact]
    public void GetResults_AfterAllQuestionsAnswered_SumsScoresAndRanks()
    {
        var game = TestGameBuilder.StartedTwoPlayerGame(out var player1, out var player2);

        //Question 1: both guess correctly -> +1 each
        var q1 = game.CurrentQuestion();
        game.RecordChoice(player1, [q1.Variant('A')]);
        game.RecordChoice(player2, [q1.Variant('B')]);
        game.RecordGuess(player1, player2, [q1.Variant('B')]); //correct
        game.RecordGuess(player2, player1, [q1.Variant('A')]); //correct
        game.AdvanceToNextQuestion();

        //Question 2: only player2 guesses correctly -> player2 +1
        var q2 = game.CurrentQuestion();
        game.RecordChoice(player1, [q2.Variant('A')]);
        game.RecordChoice(player2, [q2.Variant('B')]);
        game.RecordGuess(player1, player2, [q2.Variant('A')]); //wrong (actual B)
        game.RecordGuess(player2, player1, [q2.Variant('A')]); //correct
        game.AdvanceToNextQuestion();

        var result = game.GetResults();

        result.Scores.Single(s => s.UserId == player1.Id).TotalScore.Should().Be(1);
        result.Scores.Single(s => s.UserId == player2.Id).TotalScore.Should().Be(2);
        result.Scores.Single(s => s.UserId == player2.Id).Rank.Should().Be(1);
        result.Scores.Single(s => s.UserId == player1.Id).Rank.Should().Be(2);
    }

    [Fact]
    public void GetResults_WhenScoresTie_PlayersShareRank()
    {
        var game = TestGameBuilder.StartedTwoPlayerGame(out var player1, out var player2);

        //Both questions: everyone guesses correctly -> tie at +2 each
        game.AnswerCurrentQuestion(player1, player2);
        game.AdvanceToNextQuestion();
        game.AnswerCurrentQuestion(player1, player2);
        game.AdvanceToNextQuestion();

        var result = game.GetResults();

        result.Scores.Should().OnlyContain(s => s.TotalScore == 2);
        result.Scores.Should().OnlyContain(s => s.Rank == 1);
    }
}
