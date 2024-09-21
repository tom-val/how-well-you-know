namespace KnowMe.API.Database.Options;

public class DatabaseOptions
{
    public const string ConfigSection = "Database";

    public string? Database { get; init; }
    public string? Port { get; init; }
    public string? Host { get; init; }
    public string? User { get; init; }
    public string? Password { get; init; }

    public string ConnectionString => $"Host={Host};Port={Port};Database={Database};Uid={User};Pwd={Password};";
}
