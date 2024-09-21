namespace KnowMe.API.Domain.Entities;

public class Game
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
}
