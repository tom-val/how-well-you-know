namespace KnowMe.API.Features.Suggestions;

public record SuggestQuestionRequest(string? Language, string? Topic, IReadOnlyList<string>? Avoid);

public record QuestionSuggestionResponse(string Text, IReadOnlyList<string> Options);
