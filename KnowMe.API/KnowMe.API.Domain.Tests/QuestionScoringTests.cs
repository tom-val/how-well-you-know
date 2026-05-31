using FluentAssertions;
using KnowMe.API.Domain.Entities;

namespace KnowMe.API.Domain.Tests;

public class QuestionScoringTests
{
    [Fact]
    public void GetUserResults_WhenQuestionNotFullyAnswered_ReturnsError()
    {
        var (game, player1, _) = TwoPlayerGame();
        var question = CreateQuestion(game, player1, multipleAnswers: false, 'A', 'B', 'C');

        question.RecordChoice(QuestionUserChoice.Create(player1, question, [question.Variant('A')]).Value);

        var result = question.GetUserResults();

        result.IsSuccess.Should().BeFalse();
        result.Errors!.First().Message.Should().Be("Cannot generate results until question fully answered");
    }

    [Fact]
    public void GetUserResults_SingleAnswer_ScoresOneForCorrectGuessAndZeroForWrong()
    {
        var (game, player1, player2) = TwoPlayerGame();
        var question = CreateQuestion(game, player1, multipleAnswers: false, 'A', 'B', 'C');

        //Actual answers: player1 -> A, player2 -> B
        question.RecordChoice(QuestionUserChoice.Create(player1, question, [question.Variant('A')]).Value);
        question.RecordChoice(QuestionUserChoice.Create(player2, question, [question.Variant('B')]).Value);

        //player1 guesses player2 correctly (B); player2 guesses player1 wrong (C, actual A)
        question.RecordGuess(QuestionUserGuess.Create(player1, player2, question, [question.Variant('B')]).Value);
        question.RecordGuess(QuestionUserGuess.Create(player2, player1, question, [question.Variant('C')]).Value);

        var results = question.GetUserResults().Value;

        results.Single(r => r.UserId == player1.Id).TotalScore.Should().Be(1);
        results.Single(r => r.UserId == player2.Id).TotalScore.Should().Be(0);
    }

    [Fact]
    public void GetUserResults_MultipleAnswers_ScoresExactlyAndWithPartialPenalty()
    {
        var (game, player1, player2) = TwoPlayerGame();
        var question = CreateQuestion(game, player1, multipleAnswers: true, 'A', 'B', 'C', 'D');

        //Actual answers: player1 -> {A, B}, player2 -> {C, D}
        question.RecordChoice(QuestionUserChoice.Create(player1, question, [question.Variant('A'), question.Variant('B')]).Value);
        question.RecordChoice(QuestionUserChoice.Create(player2, question, [question.Variant('C'), question.Variant('D')]).Value);

        //player1 guesses player2 exactly {C, D} -> 3
        question.RecordGuess(QuestionUserGuess.Create(player1, player2, question, [question.Variant('C'), question.Variant('D')]).Value);
        //player2 guesses player1 {A, C}: one extra (C) + one missing (B) = 2 mistakes -> 3 - 2 = 1
        question.RecordGuess(QuestionUserGuess.Create(player2, player1, question, [question.Variant('A'), question.Variant('C')]).Value);

        var results = question.GetUserResults().Value;

        results.Single(r => r.UserId == player1.Id).TotalScore.Should().Be(3);
        results.Single(r => r.UserId == player2.Id).TotalScore.Should().Be(1);
    }

    [Fact]
    public void GetUserResults_MultipleAnswers_ClampsScoreAtZero()
    {
        var (game, player1, player2) = TwoPlayerGame();
        var question = CreateQuestion(game, player1, multipleAnswers: true, 'A', 'B', 'C', 'D');

        //Actual answers: player1 -> {A}, player2 -> {A}
        question.RecordChoice(QuestionUserChoice.Create(player1, question, [question.Variant('A')]).Value);
        question.RecordChoice(QuestionUserChoice.Create(player2, question, [question.Variant('A')]).Value);

        //player1 guesses player2 exactly {A} -> 3
        question.RecordGuess(QuestionUserGuess.Create(player1, player2, question, [question.Variant('A')]).Value);
        //player2 guesses player1 {B, C, D}: 3 extra + 1 missing (A) = 4 mistakes -> 3 - 4 = -1, clamped to 0
        question.RecordGuess(QuestionUserGuess.Create(player2, player1, question, [question.Variant('B'), question.Variant('C'), question.Variant('D')]).Value);

        var results = question.GetUserResults().Value;

        results.Single(r => r.UserId == player2.Id).TotalScore.Should().Be(0);
    }

    private static (Game game, User player1, User player2) TwoPlayerGame()
    {
        var player1 = User.Create("player1").Value;
        var player2 = User.Create("player2").Value;
        var game = Game.Create("test game", player1).Value;
        game.AddPlayer(player2);
        return (game, player1, player2);
    }

    private static Question CreateQuestion(Game game, User createdBy, bool multipleAnswers, params char[] notations)
    {
        var variants = notations.ToDictionary(n => n, n => $"Variant {n}");
        return Question.Create("Test question?", multipleAnswers, variants, createdBy, game).Value;
    }
}
