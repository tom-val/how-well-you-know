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
    /// <paramref name="avoid"/> lists questions the model should not repeat or rephrase.
    /// </summary>
    Task<QuestionSuggestionResponse?> SuggestAsync(
        string language, string? topic, IReadOnlyList<string>? avoid, CancellationToken cancellationToken);
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

    // A wide pool of subjects and framings; one of each is picked at random per call so the
    // model ranges across the whole space instead of defaulting to the same few questions.
    private static readonly string[] Topics =
    [
        "food and snacks", "drinks and coffee", "travel and faraway places", "daily routines and habits",
        "movies and TV shows", "music and concerts", "books and reading", "technology and gadgets",
        "money, shopping and splurges", "friendships and social life", "work and study life",
        "childhood memories and nostalgia", "dreams and the future", "personality quirks",
        "holidays and celebrations", "sports and the outdoors", "pets and animals",
        "fashion and personal style", "home and living spaces", "guilty pleasures",
        "fears and pet peeves", "weekends and free time", "cooking and the kitchen",
        "phones and social media", "weather and seasons", "games and competition",
        "superstitions and luck", "health and fitness", "cars and getting around",
        "art and creativity", "languages and words", "the internet and memes",
        "morning vs night routines", "embarrassing moments", "small everyday decisions",
    ];

    private static readonly string[] Angles =
    [
        "Make it a this-or-that dilemma.",
        "Phrase it as a \"who is most likely to...\" question.",
        "Make it a would-you-rather with two vivid options.",
        "Ask about a favorite or a top pick.",
        "Make it a guilty-pleasure confession.",
        "Set up a fun hypothetical scenario.",
        "Ask about a pet peeve or a deal-breaker.",
        "Ask about a quirky everyday habit.",
        "Make it a playful preference question.",
        "Ask which option they'd pick in a pinch.",
    ];

    public async Task<QuestionSuggestionResponse?> SuggestAsync(
        string language, string? topic, IReadOnlyList<string>? avoid, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_settings.ApiKey))
        {
            return null; // AI not configured — caller falls back to the static bank.
        }

        var languageName = language == "lt" ? "Lithuanian" : "English";

        var userPrompt = new StringBuilder();
        if (string.IsNullOrWhiteSpace(topic))
        {
            userPrompt.Append($"Topic: {Topics[Random.Shared.Next(Topics.Length)]}. ");
            userPrompt.Append(Angles[Random.Shared.Next(Angles.Length)]);
        }
        else
        {
            userPrompt.Append($"Topic: {topic}.");
        }

        // Steer away from anything already on screen so suggestions don't repeat.
        var recent = (avoid ?? [])
            .Where(a => !string.IsNullOrWhiteSpace(a))
            .Select(a => a.Trim())
            .TakeLast(25)
            .ToList();
        if (recent.Count > 0)
        {
            userPrompt.Append(" Do NOT repeat, translate, or closely rephrase any of these existing questions: ");
            userPrompt.Append(string.Join(" | ", recent));
            userPrompt.Append('.');
        }

        // A nonce nudges the model off identical outputs on otherwise identical inputs.
        userPrompt.Append($" Make it clearly different and a little unexpected. (variety seed {Random.Shared.Next(100000, 1000000)})");

        var systemPrompt =
            "You write questions for a party game where players guess how their friends will answer. " +
            "Generate exactly ONE short, fun, real question with 2 to 4 concise answer options. " +
            "Be creative and specific, and vary the subject and phrasing every time — avoid generic clichés " +
            "like \"What is your favorite color?\". " +
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
                new { role = "user", content = userPrompt.ToString() },
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
