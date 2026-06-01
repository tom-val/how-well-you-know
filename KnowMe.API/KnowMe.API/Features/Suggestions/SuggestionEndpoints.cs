namespace KnowMe.API.Features.Suggestions;

public static class SuggestionEndpoints
{
    public static void MapSuggestionEndpoints(this WebApplication app)
    {
        app.MapPost("/v1/questions/suggest", SuggestQuestion);
    }

    private static async Task<IResult> SuggestQuestion(
        SuggestQuestionRequest request,
        IQuestionSuggester suggester,
        CancellationToken cancellationToken)
    {
        var language = string.Equals(request.Language, "lt", StringComparison.OrdinalIgnoreCase) ? "lt" : "en";

        var suggestion = await suggester.SuggestAsync(language, request.Topic, cancellationToken);

        // 502 signals the client to fall back to its built-in static suggestion bank.
        return suggestion is null
            ? Results.StatusCode(StatusCodes.Status502BadGateway)
            : Results.Ok(suggestion);
    }
}
