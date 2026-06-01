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
    ViewerState? Viewer,
    IReadOnlyList<Guid> AwaitingPlayerIds)
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
        BuildViewer(game, viewerId),
        BuildAwaiting(game));

    private static ViewerState? BuildViewer(Game game, Guid viewerId)
    {
        var current = CurrentQuestion(game);
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

    // Players who still owe a choice and/or a guess on the current question.
    // Reveals completion status only (never answer content), so it's safe mid-question.
    private static IReadOnlyList<Guid> BuildAwaiting(Game game)
    {
        if (game.CurrentQuestionPhase != QuestionPhase.Answering)
        {
            return [];
        }

        var current = CurrentQuestion(game);
        if (current is null)
        {
            return [];
        }

        return game.Players
            .Where(p => !HasFinished(current, game.Players, p.Id))
            .Select(p => p.Id)
            .ToList();
    }

    private static bool HasFinished(Question question, IReadOnlyList<User> players, Guid userId)
    {
        if (!question.UserChoices.Any(c => c.UserId == userId))
        {
            return false;
        }

        return players
            .Where(o => o.Id != userId)
            .All(o => question.UserGuesses.Any(g => g.GuessingUserId == userId && g.ChoiceUserId == o.Id));
    }

    private static Question? CurrentQuestion(Game game) =>
        game.Status == GameStatus.Started
            ? game.Questions.FirstOrDefault(q => q.Id == game.CurrentQuestionId)
            : null;
}

// --- Summaries (list my games) ---

public record GameSummaryResponse(
    Guid GameId,
    string Name,
    string Status,
    Guid CreatedByUser,
    DateTimeOffset CreatedAt);

// --- Results ---
// Only answered questions are included, so revealing the actual picks and guesses here
// never leaks anything that's still in play.

public record ScoreResponse(Guid UserId, int TotalScore, int Rank);

public record VariantRef(string Notation, string Text);

/// <summary>What a player actually picked for a question.</summary>
public record AnswerReveal(Guid UserId, IReadOnlyList<VariantRef> Picked);

/// <summary>What a player guessed another player picked, and the points it scored.</summary>
public record GuessResultResponse(Guid ChoiceUserId, int Score, IReadOnlyList<VariantRef> Guessed);

public record PlayerQuestionResultResponse(Guid UserId, int TotalScore, IReadOnlyList<GuessResultResponse> Guesses);

public record QuestionResultResponse(
    Guid QuestionId,
    string Text,
    IReadOnlyList<AnswerReveal> Answers,
    IReadOnlyList<PlayerQuestionResultResponse> Players);

public record ResultsResponse(IReadOnlyList<ScoreResponse> Overall, IReadOnlyList<QuestionResultResponse> Questions)
{
    public static ResultsResponse From(Game game)
    {
        var overall = game.GetResults().Scores
            .Select(s => new ScoreResponse(s.UserId, s.TotalScore, s.Rank))
            .ToList();

        var questions = game.Questions
            .Where(q => q.Answered)
            .Select(BuildQuestionResult)
            .ToList();

        return new ResultsResponse(overall, questions);
    }

    private static QuestionResultResponse BuildQuestionResult(Question question)
    {
        var variantById = question.AnswerVariants.ToDictionary(
            v => v.Id,
            v => new VariantRef(v.Notation.ToString(), v.Text));

        var answers = question.UserChoices
            .Select(c => new AnswerReveal(c.UserId, MapVariants(c.SelectedVariantsIds, variantById)))
            .ToList();

        var players = question.GetUserResults().Value
            .Select(u => new PlayerQuestionResultResponse(
                u.UserId,
                u.TotalScore,
                u.GuessResults.Select(g =>
                {
                    var guess = question.UserGuesses
                        .First(x => x.GuessingUserId == g.GuessingUser && x.ChoiceUserId == g.ChoiceUser);
                    return new GuessResultResponse(g.ChoiceUser, g.Score, MapVariants(guess.SelectedVariantsIds, variantById));
                }).ToList()))
            .ToList();

        return new QuestionResultResponse(question.Id, question.Text, answers, players);
    }

    private static IReadOnlyList<VariantRef> MapVariants(
        IEnumerable<Guid> variantIds, Dictionary<Guid, VariantRef> variantById) =>
        variantIds
            .Where(variantById.ContainsKey)
            .Select(id => variantById[id])
            .OrderBy(v => v.Notation)
            .ToList();
}
