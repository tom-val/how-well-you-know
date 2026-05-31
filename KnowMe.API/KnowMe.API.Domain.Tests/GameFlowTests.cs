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

    [Fact]
    public void ThreePlayersPlayFullGameFromStartToFinish()
    {
        // --- Setup: three users ---
        var p1 = User.Create("player1").Value;
        var p2 = User.Create("player2").Value;
        var p3 = User.Create("player3").Value;

        // --- Create game (creator is the first player) ---
        var game = Game.Create("Quiz night", p1).Value;
        game.Status.Should().Be(GameStatus.Created);
        game.Players.Should().ContainSingle();

        // --- Others join ---
        game.AddPlayer(p2).IsSuccess.Should().BeTrue();
        game.AddPlayer(p3).IsSuccess.Should().BeTrue();
        game.Players.Should().HaveCount(3);

        // --- Add a single-answer and a multiple-answer question ---
        var singleVariants = new Dictionary<char, string> { { 'A', "Red" }, { 'B', "Green" }, { 'C', "Blue" } };
        game.AddQuestion(Question.Create("Favourite colour?", false, singleVariants, p1, game).Value)
            .IsSuccess.Should().BeTrue();

        var multipleVariants = new Dictionary<char, string> { { 'A', "Coffee" }, { 'B', "Tea" }, { 'C', "Water" }, { 'D', "Juice" } };
        game.AddQuestion(Question.Create("Which drinks do you like?", true, multipleVariants, p2, game).Value)
            .IsSuccess.Should().BeTrue();
        game.Questions.Should().HaveCount(2);

        // --- Start ---
        var startResult = game.StartGame();
        startResult.IsSuccess.Should().BeTrue();
        game.Status.Should().Be(GameStatus.Started);
        game.CurrentQuestionPhase.Should().Be(QuestionPhase.Answering);
        game.CurrentQuestionId.Should().NotBe(Guid.Empty);

        // Question order is by id, so play whichever phase is current first.
        if (game.CurrentQuestion().MultipleAnswers)
        {
            PlayMultipleAnswerQuestion();
            PlaySingleAnswerQuestion();
        }
        else
        {
            PlaySingleAnswerQuestion();
            PlayMultipleAnswerQuestion();
        }

        // --- Game finished ---
        game.Status.Should().Be(GameStatus.Ended);

        // --- Final aggregated leaderboard: p1=2+6=8, p2=1+4=5, p3=1+4=5 ---
        var finalResults = game.GetResults();
        finalResults.GameId.Should().Be(game.Id);
        finalResults.Scores.Single(s => s.UserId == p1.Id).TotalScore.Should().Be(8);
        finalResults.Scores.Single(s => s.UserId == p2.Id).TotalScore.Should().Be(5);
        finalResults.Scores.Single(s => s.UserId == p3.Id).TotalScore.Should().Be(5);
        finalResults.Scores.Single(s => s.UserId == p1.Id).Rank.Should().Be(1);
        finalResults.Scores.Single(s => s.UserId == p2.Id).Rank.Should().Be(2);
        finalResults.Scores.Single(s => s.UserId == p3.Id).Rank.Should().Be(2);
        return;

        void PlaySingleAnswerQuestion()
        {
            var q = game.CurrentQuestion();
            q.MultipleAnswers.Should().BeFalse();

            // Actual answers: p1 -> A, p2 -> B, p3 -> C
            game.RecordChoice(p1, [q.Variant('A')]).IsSuccess.Should().BeTrue();
            game.RecordChoice(p2, [q.Variant('B')]).IsSuccess.Should().BeTrue();
            game.RecordChoice(p3, [q.Variant('C')]).IsSuccess.Should().BeTrue();

            // Guesses (correct unless noted)
            game.RecordGuess(p1, p2, [q.Variant('B')]).IsSuccess.Should().BeTrue();          // correct
            game.RecordGuess(p1, p3, [q.Variant('C')]).IsSuccess.Should().BeTrue();          // correct
            game.RecordGuess(p2, p1, [q.Variant('A')]).IsSuccess.Should().BeTrue();          // correct
            game.RecordGuess(p2, p3, [q.Variant('A')]).IsSuccess.Should().BeTrue();          // wrong (actual C)
            game.RecordGuess(p3, p1, [q.Variant('B')]).IsSuccess.Should().BeTrue();          // wrong (actual A)
            game.RecordGuess(p3, p2, [q.Variant('B')]).IsSuccess.Should().BeTrue();          // correct

            // Last guess drives the question into review without ending the game
            game.CurrentQuestionPhase.Should().Be(QuestionPhase.Review);
            game.Status.Should().Be(GameStatus.Started);

            // Per-question results: p1=2, p2=1, p3=1
            var results = q.GetUserResults();
            results.IsSuccess.Should().BeTrue();
            results.Value.Single(r => r.UserId == p1.Id).TotalScore.Should().Be(2);
            results.Value.Single(r => r.UserId == p2.Id).TotalScore.Should().Be(1);
            results.Value.Single(r => r.UserId == p3.Id).TotalScore.Should().Be(1);

            game.AdvanceToNextQuestion().IsSuccess.Should().BeTrue();
        }

        void PlayMultipleAnswerQuestion()
        {
            var q = game.CurrentQuestion();
            q.MultipleAnswers.Should().BeTrue();

            // Actual answers: p1 -> {A,B}, p2 -> {C}, p3 -> {A,D}
            game.RecordChoice(p1, [q.Variant('A'), q.Variant('B')]).IsSuccess.Should().BeTrue();
            game.RecordChoice(p2, [q.Variant('C')]).IsSuccess.Should().BeTrue();
            game.RecordChoice(p3, [q.Variant('A'), q.Variant('D')]).IsSuccess.Should().BeTrue();

            // Scoring: 3 - (extras + misses), floored at 0
            game.RecordGuess(p1, p2, [q.Variant('C')]).IsSuccess.Should().BeTrue();                               // exact -> 3
            game.RecordGuess(p1, p3, [q.Variant('A'), q.Variant('D')]).IsSuccess.Should().BeTrue();               // exact -> 3
            game.RecordGuess(p2, p1, [q.Variant('A')]).IsSuccess.Should().BeTrue();                               // miss B -> 2
            game.RecordGuess(p2, p3, [q.Variant('A'), q.Variant('B'), q.Variant('D')]).IsSuccess.Should().BeTrue(); // extra B -> 2
            game.RecordGuess(p3, p1, [q.Variant('A'), q.Variant('B')]).IsSuccess.Should().BeTrue();               // exact -> 3
            game.RecordGuess(p3, p2, [q.Variant('D')]).IsSuccess.Should().BeTrue();                               // extra D, miss C -> 1

            game.CurrentQuestionPhase.Should().Be(QuestionPhase.Review);

            // Per-question results: p1=3+3=6, p2=2+2=4, p3=3+1=4
            var results = q.GetUserResults().Value;
            results.Single(r => r.UserId == p1.Id).TotalScore.Should().Be(6);
            results.Single(r => r.UserId == p2.Id).TotalScore.Should().Be(4);
            results.Single(r => r.UserId == p3.Id).TotalScore.Should().Be(4);

            game.AdvanceToNextQuestion().IsSuccess.Should().BeTrue();
        }
    }
}
