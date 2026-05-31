using KnowMe.API.Domain.Entities;

namespace KnowMe.API.Domain.Tests;

internal static class TestGameBuilder
{
    /// <summary>
    /// Builds a started two-player game with two single-answer questions,
    /// positioned on the first question in the answering phase.
    /// </summary>
    public static Game StartedTwoPlayerGame(out User player1, out User player2)
    {
        player1 = User.Create("player1").Value;
        player2 = User.Create("player2").Value;

        var game = Game.Create("test game", player1).Value;
        game.AddPlayer(player2);

        var question1Variants = new Dictionary<char, string>
        {
            { 'A', "Blue" },
            { 'B', "Green" },
            { 'C', "Yellow" }
        };
        game.AddQuestion(Question.Create("Favourite colour?", false, question1Variants, player1, game).Value);

        var question2Variants = new Dictionary<char, string>
        {
            { 'A', "Vilnius" },
            { 'B', "Kaunas" },
            { 'C', "Klaipeda" }
        };
        game.AddQuestion(Question.Create("Best place to be?", false, question2Variants, player2, game).Value);

        game.StartGame();
        return game;
    }

    public static Question CurrentQuestion(this Game game) =>
        game.Questions.First(q => q.Id == game.CurrentQuestionId);

    public static QuestionVariant Variant(this Question question, char notation) =>
        question.AnswerVariants.First(v => v.Notation == notation);

    /// <summary>
    /// Both players answer the current question and guess each other, driving it into review.
    /// Each player picks variant 'A' for their own answer and guesses 'A' for the other.
    /// </summary>
    public static void AnswerCurrentQuestion(this Game game, User player1, User player2)
    {
        var question = game.CurrentQuestion();

        game.RecordChoice(player1, [question.Variant('A')]);
        game.RecordChoice(player2, [question.Variant('A')]);

        game.RecordGuess(player1, player2, [question.Variant('A')]);
        game.RecordGuess(player2, player1, [question.Variant('A')]);
    }
}
