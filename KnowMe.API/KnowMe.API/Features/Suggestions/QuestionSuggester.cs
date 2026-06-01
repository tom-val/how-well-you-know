using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace KnowMe.API.Features.Suggestions;

public interface IQuestionSuggester
{
    /// <summary>
    /// Generates one question via the model, or returns null if AI is unavailable
    /// (no key, provider error, timeout, or malformed output) so the caller can fall back.
    /// </summary>
    Task<QuestionSuggestionResponse?> SuggestAsync(string language, string? topic, CancellationToken cancellationToken);
}

public class OpenAiQuestionSuggester : IQuestionSuggester
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _http;
    private readonly OpenAiSettings _settings;
    private readonly ILogger<OpenAiQuestionSuggester> _logger;

    public OpenAiQuestionSuggester(
        HttpClient http,
        IOptions<OpenAiSettings> settings,
        ILogger<OpenAiQuestionSuggester> logger)
    {
        _http = http;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<QuestionSuggestionResponse?> SuggestAsync(
        string language, string? topic, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_settings.ApiKey))
        {
            return null; // AI not configured — caller falls back to the static bank.
        }

        var languageName = language == "lt" ? "Lithuanian" : "English";
        var topicLine = string.IsNullOrWhiteSpace(topic)
            ? "Pick any everyday topic: food, drinks, travel, hobbies, preferences, this-or-that."
            : $"Topic: {topic}.";

        var systemPrompt =
            "You write questions for a party game where players guess how their friends will answer. " +
            "Generate exactly ONE short, fun, real question with 2 to 4 concise answer options. " +
            "The question must be at most 80 characters; each option at most 40 characters. " +
            $"Write the question and all options in {languageName}. " +
            "Respond with ONLY a JSON object of the form {\"text\": string, \"options\": [string, ...]}.";

        var payload = new
        {
            model = _settings.Model,
            response_format = new { type = "json_object" },
            messages = new object[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = topicLine },
            },
        };

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, $"{_settings.BaseUrl}/chat/completions")
            {
                Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"),
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.ApiKey);

            using var response = await _http.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("[AiSuggest] Provider returned {Status}.", (int)response.StatusCode);
                return null;
            }

            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            var content = ExtractMessageContent(body);
            return content is null ? null : ParseSuggestion(content);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[AiSuggest] Suggestion request failed.");
            return null;
        }
    }

    private static string? ExtractMessageContent(string responseBody)
    {
        using var doc = JsonDocument.Parse(responseBody);
        return doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();
    }

    private QuestionSuggestionResponse? ParseSuggestion(string content)
    {
        // The model is asked for pure JSON, but defend against fences / stray text.
        var start = content.IndexOf('{');
        var end = content.LastIndexOf('}');
        if (start < 0 || end <= start)
        {
            return null;
        }

        var json = content.Substring(start, end - start + 1);

        Suggestion? parsed;
        try
        {
            parsed = JsonSerializer.Deserialize<Suggestion>(json, JsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }

        var text = parsed?.Text?.Trim();
        var options = parsed?.Options?
            .Where(o => !string.IsNullOrWhiteSpace(o))
            .Select(o => o.Trim())
            .ToList() ?? [];

        // Keep within the domain's own limits (question <= 100, option <= 100, 2..20 options).
        if (string.IsNullOrEmpty(text) || text.Length > 100 || options.Count is < 2 or > 20
            || options.Any(o => o.Length > 100))
        {
            _logger.LogWarning("[AiSuggest] Discarded malformed/oversized suggestion.");
            return null;
        }

        return new QuestionSuggestionResponse(text, options);
    }

    private record Suggestion(string? Text, List<string>? Options);
}
