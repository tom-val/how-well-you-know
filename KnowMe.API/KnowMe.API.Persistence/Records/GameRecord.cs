namespace KnowMe.API.Persistence.Records;

/// <summary>
/// Flat, JSON-friendly representations of the Game aggregate as stored in DynamoDB.
/// These mirror the domain entities but carry no behaviour, so the whole aggregate
/// can be (de)serialised to a single document attribute.
/// </summary>
public record GameRecord
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
    public Guid CreatedByUser { get; init; }
    public string Status { get; init; } = string.Empty;
    public string CurrentQuestionPhase { get; init; } = string.Empty;
    public Guid CurrentQuestionId { get; init; }
    public List<UserRecord> Players { get; init; } = [];
    public List<QuestionRecord> Questions { get; init; } = [];
}

public record UserRecord
{
    public Guid Id { get; init; }
    public string UserName { get; init; } = string.Empty;
    public string? ProfileUrl { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}

public record QuestionRecord
{
    public Guid Id { get; init; }
    public string Text { get; init; } = string.Empty;
    public bool MultipleAnswers { get; init; }
    public Guid CreatedByUser { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public Guid GameId { get; init; }
    public List<VariantRecord> Variants { get; init; } = [];
    public List<ChoiceRecord> Choices { get; init; } = [];
    public List<GuessRecord> Guesses { get; init; } = [];
}

public record VariantRecord
{
    public Guid Id { get; init; }
    public Guid QuestionId { get; init; }
    public string Text { get; init; } = string.Empty;
    public char Notation { get; init; }
}

public record ChoiceRecord
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public Guid QuestionId { get; init; }
    public List<Guid> SelectedVariantsIds { get; init; } = [];
}

public record GuessRecord
{
    public Guid Id { get; init; }
    public Guid GuessingUserId { get; init; }
    public Guid ChoiceUserId { get; init; }
    public Guid QuestionId { get; init; }
    public List<Guid> SelectedVariantsIds { get; init; } = [];
}
