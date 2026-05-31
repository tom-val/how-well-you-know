using KnowMe.API.Domain.Entities;
using KnowMe.API.Features.Users;

namespace KnowMe.API.Features.Games;

// --- Requests ---

public record CreateGameRequest(string Name);

public record AddQuestionRequest(string Text, bool MultipleAnswers, Dictionary<string, string> Variants);

public record RecordChoiceRequest(string[] VariantNotations);

public record RecordGuessRequest(Guid ChoiceUserId, string[] VariantNotations);

// --- Responses ---

public record VariantResponse(Guid Id, string Notation, string Text);

public record QuestionResponse(
    Guid Id,
    string Text,
    bool MultipleAnswers,
    Guid CreatedByUser,
    bool Answered,
    IReadOnlyList<VariantResponse> Variants)
{
    public static QuestionResponse From(Question question) => new(
        question.Id,
        question.Text,
        question.MultipleAnswers,
        question.CreatedByUser,
        question.Answered,
        question.AnswerVariants
            .OrderBy(v => v.Notation)
            .Select(v => new VariantResponse(v.Id, v.Notation.ToString(), v.Text))
            .ToList());
}

/// <summary>
/// Game projection for clients. Deliberately excludes individual choices/guesses so
/// players cannot see each other's answers while a question is still being answered.
/// </summary>
public record GameResponse(
    Guid Id,
    string Name,
    string Status,
    string CurrentQuestionPhase,
    Guid CurrentQuestionId,
    Guid CreatedByUser,
    IReadOnlyList<UserResponse> Players,
    IReadOnlyList<QuestionResponse> Questions)
{
    public static GameResponse From(Game game) => new(
        game.Id,
        game.Name,
        game.Status.ToString(),
        game.CurrentQuestionPhase.ToString(),
        game.CurrentQuestionId,
        game.CreatedByUser,
        game.Players.Select(UserResponse.From).ToList(),
        game.Questions.Select(QuestionResponse.From).ToList());
}

// --- Summaries (list my games) ---

public record GameSummaryResponse(
    Guid GameId,
    string Name,
    string Status,
    Guid CreatedByUser,
    DateTimeOffset CreatedAt);

// --- Results ---

public record ScoreResponse(Guid UserId, int TotalScore, int Rank);

public record GuessResultResponse(Guid ChoiceUserId, int Score);

public record PlayerQuestionResultResponse(Guid UserId, int TotalScore, IReadOnlyList<GuessResultResponse> Guesses);

public record QuestionResultResponse(Guid QuestionId, string Text, IReadOnlyList<PlayerQuestionResultResponse> Players);

public record ResultsResponse(IReadOnlyList<ScoreResponse> Overall, IReadOnlyList<QuestionResultResponse> Questions)
{
    public static ResultsResponse From(Game game)
    {
        var overall = game.GetResults().Scores
            .Select(s => new ScoreResponse(s.UserId, s.TotalScore, s.Rank))
            .ToList();

        var questions = game.Questions
            .Where(q => q.Answered)
            .Select(q => new QuestionResultResponse(
                q.Id,
                q.Text,
                q.GetUserResults().Value
                    .Select(u => new PlayerQuestionResultResponse(
                        u.UserId,
                        u.TotalScore,
                        u.GuessResults.Select(g => new GuessResultResponse(g.ChoiceUser, g.Score)).ToList()))
                    .ToList()))
            .ToList();

        return new ResultsResponse(overall, questions);
    }
}
