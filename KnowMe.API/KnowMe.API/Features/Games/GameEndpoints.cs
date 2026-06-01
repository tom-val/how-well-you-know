using FluentValidation;
using KnowMe.API.Domain.Entities;
using KnowMe.API.Persistence.Repositories;
using KnowMe.API.Shared;
using KnowMe.API.Shared.Extensions;

namespace KnowMe.API.Features.Games;

public static class GameEndpoints
{
    public static void MapGameEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/v1/games");

        group.MapPost("/", CreateGame);
        group.MapGet("/mine", ListMyGames);
        group.MapGet("/{id:guid}", GetGame);
        group.MapGet("/{id:guid}/results", GetResults);
        group.MapPost("/{id:guid}/join", JoinGame);
        group.MapPost("/{id:guid}/questions", AddQuestion);
        group.MapDelete("/{id:guid}/questions/{questionId:guid}", RemoveQuestion);
        group.MapPost("/{id:guid}/start", StartGame);
        group.MapPost("/{id:guid}/choices", RecordChoice);
        group.MapPost("/{id:guid}/guesses", RecordGuess);
        group.MapPost("/{id:guid}/advance", AdvanceQuestion);
    }

    private static async Task<IResult> CreateGame(
        CreateGameRequest request,
        IValidator<CreateGameRequest> validator,
        IGameRepository games,
        IUserRepository users,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return Results.ValidationProblem(validation.ToDictionary());

        var creator = await users.GetAsync(context.GetUserId(), cancellationToken);
        if (creator is null)
            return Results.NotFound(new { error = "Acting user not found." });

        var result = Game.Create(request.Name, creator);
        if (!result.IsSuccess)
            return ApiResults.DomainValidationProblem(result.Errors!);

        await games.SaveAsync(result.Value, cancellationToken);
        return Results.Created($"/v1/games/{result.Value.Id}", GameResponse.From(result.Value, context.GetUserId()));
    }

    private static async Task<IResult> GetGame(
        Guid id,
        IGameRepository games,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var game = await games.GetAsync(id, cancellationToken);
        return game is null
            ? Results.NotFound(new { error = "Game not found." })
            : Results.Ok(GameResponse.From(game, context.GetUserId()));
    }

    // Lists every game the current user has created or joined (newest first).
    private static async Task<IResult> ListMyGames(
        IGameRepository games,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var memberships = await games.ListForUserAsync(context.GetUserId(), cancellationToken);

        var summaries = memberships
            .OrderByDescending(m => m.CreatedAt)
            .Select(m => new GameSummaryResponse(m.GameId, m.Name, m.Status, m.CreatedByUser, m.CreatedAt, m.QuestionCount, m.PlayerCount))
            .ToList();

        return Results.Ok(summaries);
    }

    private static async Task<IResult> GetResults(Guid id, IGameRepository games, CancellationToken cancellationToken)
    {
        var game = await games.GetAsync(id, cancellationToken);
        return game is null
            ? Results.NotFound(new { error = "Game not found." })
            : Results.Ok(ResultsResponse.From(game));
    }

    private static async Task<IResult> JoinGame(
        Guid id,
        IGameRepository games,
        IUserRepository users,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var game = await games.GetAsync(id, cancellationToken);
        if (game is null)
            return Results.NotFound(new { error = "Game not found." });

        var user = await users.GetAsync(context.GetUserId(), cancellationToken);
        if (user is null)
            return Results.NotFound(new { error = "Acting user not found." });

        var result = game.AddPlayer(user);
        if (!result.IsSuccess)
            return ApiResults.DomainValidationProblem(result.Errors!);

        await games.SaveAsync(game, cancellationToken);
        return Results.Ok(GameResponse.From(game, context.GetUserId()));
    }

    private static async Task<IResult> AddQuestion(
        Guid id,
        AddQuestionRequest request,
        IValidator<AddQuestionRequest> validator,
        IGameRepository games,
        IUserRepository users,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return Results.ValidationProblem(validation.ToDictionary());

        var game = await games.GetAsync(id, cancellationToken);
        if (game is null)
            return Results.NotFound(new { error = "Game not found." });

        var creator = await users.GetAsync(context.GetUserId(), cancellationToken);
        if (creator is null)
            return Results.NotFound(new { error = "Acting user not found." });

        var variants = request.Variants.ToDictionary(kvp => kvp.Key[0], kvp => kvp.Value);

        var questionResult = Question.Create(request.Text, request.MultipleAnswers, variants, creator, game);
        if (!questionResult.IsSuccess)
            return ApiResults.DomainValidationProblem(questionResult.Errors!);

        var addResult = game.AddQuestion(questionResult.Value);
        if (!addResult.IsSuccess)
            return ApiResults.DomainValidationProblem(addResult.Errors!);

        await games.SaveAsync(game, cancellationToken);
        return Results.Created($"/v1/games/{game.Id}", QuestionResponse.From(questionResult.Value));
    }

    private static async Task<IResult> RemoveQuestion(
        Guid id,
        Guid questionId,
        IGameRepository games,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var game = await games.GetAsync(id, cancellationToken);
        if (game is null)
            return Results.NotFound(new { error = "Game not found." });

        var result = game.RemoveQuestion(questionId);
        if (!result.IsSuccess)
            return ApiResults.DomainValidationProblem(result.Errors!);

        await games.SaveAsync(game, cancellationToken);
        return Results.Ok(GameResponse.From(game, context.GetUserId()));
    }

    private static async Task<IResult> StartGame(
        Guid id,
        IGameRepository games,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var game = await games.GetAsync(id, cancellationToken);
        if (game is null)
            return Results.NotFound(new { error = "Game not found." });

        var result = game.StartGame();
        if (!result.IsSuccess)
            return ApiResults.DomainValidationProblem(result.Errors!);

        await games.SaveAsync(game, cancellationToken);
        return Results.Ok(GameResponse.From(game, context.GetUserId()));
    }

    private static async Task<IResult> RecordChoice(
        Guid id,
        RecordChoiceRequest request,
        IValidator<RecordChoiceRequest> validator,
        IGameRepository games,
        IUserRepository users,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return Results.ValidationProblem(validation.ToDictionary());

        var game = await games.GetAsync(id, cancellationToken);
        if (game is null)
            return Results.NotFound(new { error = "Game not found." });

        var user = await ResolveUserAsync(game, context.GetUserId(), users, cancellationToken);
        if (user is null)
            return Results.NotFound(new { error = "Acting user not found." });

        if (!TryResolveCurrentQuestionVariants(game, request.VariantNotations, out var variants, out var error))
            return ApiResults.DomainValidationProblem([new() { Message = error! }]);

        var result = game.RecordChoice(user, variants);
        if (!result.IsSuccess)
            return ApiResults.DomainValidationProblem(result.Errors!);

        await games.SaveAsync(game, cancellationToken);
        return Results.Ok(GameResponse.From(game, context.GetUserId()));
    }

    private static async Task<IResult> RecordGuess(
        Guid id,
        RecordGuessRequest request,
        IValidator<RecordGuessRequest> validator,
        IGameRepository games,
        IUserRepository users,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return Results.ValidationProblem(validation.ToDictionary());

        var game = await games.GetAsync(id, cancellationToken);
        if (game is null)
            return Results.NotFound(new { error = "Game not found." });

        var guessingUser = await ResolveUserAsync(game, context.GetUserId(), users, cancellationToken);
        if (guessingUser is null)
            return Results.NotFound(new { error = "Acting user not found." });

        var choiceUser = await ResolveUserAsync(game, request.ChoiceUserId, users, cancellationToken);
        if (choiceUser is null)
            return ApiResults.DomainValidationProblem([new() { Message = "Choice user not found." }]);

        if (!TryResolveCurrentQuestionVariants(game, request.VariantNotations, out var variants, out var error))
            return ApiResults.DomainValidationProblem([new() { Message = error! }]);

        var result = game.RecordGuess(guessingUser, choiceUser, variants);
        if (!result.IsSuccess)
            return ApiResults.DomainValidationProblem(result.Errors!);

        await games.SaveAsync(game, cancellationToken);
        return Results.Ok(GameResponse.From(game, context.GetUserId()));
    }

    private static async Task<IResult> AdvanceQuestion(
        Guid id,
        IGameRepository games,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var game = await games.GetAsync(id, cancellationToken);
        if (game is null)
            return Results.NotFound(new { error = "Game not found." });

        var result = game.AdvanceToNextQuestion();
        if (!result.IsSuccess)
            return ApiResults.DomainValidationProblem(result.Errors!);

        await games.SaveAsync(game, cancellationToken);
        return Results.Ok(GameResponse.From(game, context.GetUserId()));
    }

    // Prefer the player instance already on the aggregate; fall back to a lookup for not-yet-joined users.
    private static async Task<User?> ResolveUserAsync(
        Game game, Guid userId, IUserRepository users, CancellationToken cancellationToken)
    {
        return game.Players.FirstOrDefault(p => p.Id == userId)
            ?? await users.GetAsync(userId, cancellationToken);
    }

    private static bool TryResolveCurrentQuestionVariants(
        Game game,
        IEnumerable<string> notations,
        out List<QuestionVariant> variants,
        out string? error)
    {
        variants = [];

        var currentQuestion = game.Questions.FirstOrDefault(q => q.Id == game.CurrentQuestionId);
        if (currentQuestion is null)
        {
            error = "There is no active question to answer.";
            return false;
        }

        foreach (var notation in notations)
        {
            var variant = currentQuestion.AnswerVariants
                .FirstOrDefault(v => string.Equals(v.Notation.ToString(), notation, StringComparison.OrdinalIgnoreCase));

            if (variant is null)
            {
                error = $"Unknown variant notation '{notation}'.";
                return false;
            }

            variants.Add(variant);
        }

        error = null;
        return true;
    }
}
