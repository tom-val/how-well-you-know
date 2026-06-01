namespace KnowMe.API.Features.Suggestions;

public record SuggestQuestionRequest(string? Language, string? Topic);

public record QuestionSuggestionResponse(string Text, IReadOnlyList<string> Options);
