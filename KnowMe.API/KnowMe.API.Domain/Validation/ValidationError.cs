namespace KnowMe.API.Domain.Validation;

public class ValidationError
{
    public required string Message { get; init; }
    public string? ValidationDetails { get; init; }
}
