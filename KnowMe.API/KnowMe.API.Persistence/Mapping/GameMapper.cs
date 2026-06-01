using KnowMe.API.Domain.Entities;
using KnowMe.API.Domain.Enums;
using KnowMe.API.Persistence.Records;

namespace KnowMe.API.Persistence.Mapping;

/// <summary>
/// Translates between the Game aggregate and its persisted record form.
/// Uses the domain's internal rehydration factories to rebuild entities with
/// their original identifiers and state (no validation, no new ids).
/// </summary>
internal static class GameMapper
{
    public static GameRecord ToRecord(Game game) => new()
    {
        Id = game.Id,
        Name = game.Name,
        CreatedAt = game.CreatedAt,
        CreatedByUser = game.CreatedByUser,
        Status = game.Status.ToString(),
        CurrentQuestionPhase = game.CurrentQuestionPhase.ToString(),
        CurrentQuestionId = game.CurrentQuestionId,
        Players = game.Players.Select(UserMapper.ToRecord).ToList(),
        Questions = game.Questions.Select(ToRecord).ToList(),
        ReadyUserIds = game.ReadyUserIds.ToList()
    };

    public static Game ToDomain(GameRecord record)
    {
        var players = record.Players.Select(UserMapper.ToDomain).ToList();
        var questions = record.Questions.Select(ToDomain).ToList();

        return Game.Rehydrate(
            record.Id,
            record.Name,
            record.CreatedAt,
            players,
            questions,
            record.CurrentQuestionId,
            record.CreatedByUser,
            Enum.Parse<GameStatus>(record.Status),
            Enum.Parse<QuestionPhase>(record.CurrentQuestionPhase),
            record.ReadyUserIds.ToList());
    }

    private static QuestionRecord ToRecord(Question question) => new()
    {
        Id = question.Id,
        Text = question.Text,
        MultipleAnswers = question.MultipleAnswers,
        Order = question.Order,
        CreatedByUser = question.CreatedByUser,
        CreatedAt = question.CreatedAt,
        GameId = question.GameId,
        Variants = question.AnswerVariants.Select(v => new VariantRecord
        {
            Id = v.Id,
            QuestionId = v.QuestionId,
            Text = v.Text,
            Notation = v.Notation
        }).ToList(),
        Choices = question.UserChoices.Select(c => new ChoiceRecord
        {
            Id = c.Id,
            UserId = c.UserId,
            QuestionId = c.QuestionId,
            SelectedVariantsIds = c.SelectedVariantsIds.ToList()
        }).ToList(),
        Guesses = question.UserGuesses.Select(g => new GuessRecord
        {
            Id = g.Id,
            GuessingUserId = g.GuessingUserId,
            ChoiceUserId = g.ChoiceUserId,
            QuestionId = g.QuestionId,
            SelectedVariantsIds = g.SelectedVariantsIds.ToList()
        }).ToList()
    };

    private static Question ToDomain(QuestionRecord record)
    {
        var variants = record.Variants
            .Select(v => QuestionVariant.Rehydrate(v.Id, v.QuestionId, v.Text, v.Notation))
            .ToList();

        var choices = record.Choices
            .Select(c => QuestionUserChoice.Rehydrate(c.Id, c.UserId, c.QuestionId, c.SelectedVariantsIds.ToList()))
            .ToList();

        var guesses = record.Guesses
            .Select(g => QuestionUserGuess.Rehydrate(
                g.Id, g.GuessingUserId, g.ChoiceUserId, g.QuestionId, g.SelectedVariantsIds.ToList()))
            .ToList();

        return Question.Rehydrate(
            record.Id,
            record.Text,
            record.MultipleAnswers,
            record.Order,
            record.CreatedByUser,
            record.CreatedAt,
            record.GameId,
            variants,
            choices,
            guesses);
    }
}

internal static class UserMapper
{
    public static UserRecord ToRecord(User user) => new()
    {
        Id = user.Id,
        UserName = user.UserName,
        ProfileUrl = user.ProfileUrl,
        CreatedAt = user.CreatedAt
    };

    public static User ToDomain(UserRecord record) =>
        User.Rehydrate(record.Id, record.UserName, record.ProfileUrl, record.CreatedAt);
}
