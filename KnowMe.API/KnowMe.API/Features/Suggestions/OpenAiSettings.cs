namespace KnowMe.API.Features.Suggestions;

public class OpenAiSettings
{
    public const string SectionName = "OpenAi";

    /// <summary>API key. When empty, AI suggestions are disabled (the client falls back to the static bank).</summary>
    public string ApiKey { get; set; } = "";

    public string BaseUrl { get; set; } = "https://api.openai.com/v1";

    public string Model { get; set; } = "gpt-5.4-mini";
}
