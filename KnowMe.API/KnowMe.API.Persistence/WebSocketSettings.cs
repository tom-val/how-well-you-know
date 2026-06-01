namespace KnowMe.API.Persistence;

public class WebSocketSettings
{
    public const string SectionName = "WebSocket";

    /// <summary>
    /// https:// API Gateway Management endpoint used to push messages to connected sockets.
    /// Empty disables broadcasting (e.g. local development and tests).
    /// </summary>
    public string ManagementEndpoint { get; set; } = "";
}
