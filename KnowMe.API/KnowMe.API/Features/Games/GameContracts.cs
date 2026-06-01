using KnowMe.API.Domain.Entities;
using KnowMe.API.Domain.Enums;
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
/// The calling player's own progress on the current question. Reveals only the caller's
/// own actions (never anyone else's answers), so it's safe to return mid-question and lets
/// the UI recover state after a refresh.
/// </summary>
public record ViewerState(bool HasAnswered, IReadOnlyList<Guid> GuessedUserIds);

/// <summary>
/// Game projection for clients. Deliberately excludes other players' choices/guesses so
/// nobody can see each other's answers while a question is still being answered.
/// </summary>
public record GameResponse(
    Guid Id,
    string Name,
    string Status,
    string CurrentQuestionPhase,
    Guid CurrentQuestionId,
    Guid CreatedByUser,
    IReadOnlyList<UserResponse> Players,
    IReadOnlyList<QuestionResponse> Questions,
    ViewerState? Viewer)
{
    public static GameResponse From(Game game, Guid viewerId) => new(
        game.Id,
        game.Name,
        game.Status.ToString(),
        game.CurrentQuestionPhase.ToString(),
        game.CurrentQuestionId,
        game.CreatedByUser,
        game.Players.Select(UserResponse.From).ToList(),
        game.Questions.Select(QuestionResponse.From).ToList(),
        BuildViewer(game, viewerId));

    private static ViewerState? BuildViewer(Game game, Guid viewerId)
    {
        if (game.Status != GameStatus.Started)
        {
            return null;
        }

        var current = game.Questions.FirstOrDefault(q => q.Id == game.CurrentQuestionId);
        if (current is null)
        {
            return null;
        }

        return new ViewerState(
            current.UserChoices.Any(c => c.UserId == viewerId),
            current.UserGuesses
                .Where(g => g.GuessingUserId == viewerId)
                .Select(g => g.ChoiceUserId)
                .ToList());
    }
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
