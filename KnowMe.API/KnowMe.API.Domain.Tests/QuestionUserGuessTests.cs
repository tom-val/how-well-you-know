using FluentAssertions;
using KnowMe.API.Domain.Entities;

namespace KnowMe.API.Domain.Tests;

public class QuestionUserGuessTests
{
    [Fact]
    public void Create_GivenOneChoiceForNonMultipleAnswersQuestion_CreatesIt()
    {
        var (user, question) = CreateQuestion();
        var (user2, _) = CreateQuestion();

        var userChoice = QuestionUserGuess.Create(user, user2, question, [question.AnswerVariants.First(v => v.Notation == 'A')]);
        userChoice.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Create_GivenChoiceFromDifferentQuestion_ReturnsError()
    {
        var (user, question) = CreateQuestion();
        var (user2, question2) = CreateQuestion();

        var userChoice = QuestionUserGuess.Create(user, user2, question, [question2.AnswerVariants.First(v => v.Notation == 'A')]);
        userChoice.IsSuccess.Should().BeFalse();
        userChoice.Errors!.First().Message.Should().Be("Selected variant must belong to question");
    }

    [Fact]
    public void Create_GivenMultipleVariantsForNonMultipleAnswersQuestion_ReturnsError()
    {
        var (user, question) = CreateQuestion();
        var (user2, _) = CreateQuestion();

        var userChoice = QuestionUserGuess.Create(user, user2, question, [question.AnswerVariants.First(v => v.Notation == 'A'), question.AnswerVariants.First(v => v.Notation == 'B')]);
        userChoice.IsSuccess.Should().BeFalse();
        userChoice.Errors!.First().Message.Should().Be("Can select only one variant when not multiple answers");
    }

    private static (User user , Question question) CreateQuestion()
    {
        var user = User.Create("player1").Value;
        var gameResult = Game.Create("test game", user).Value;

        var questionVariants = new Dictionary<char, string>()
        {
            { 'A', "Yes" },
            { 'B', "No" }
        };

        return (user, Question.Create("Is this the real life?", false, questionVariants, user, gameResult).Value);
    }
}
