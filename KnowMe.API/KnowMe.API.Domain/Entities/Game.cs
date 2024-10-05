using KnowMe.API.Domain.Enums;
using KnowMe.API.Domain.Validation;

namespace KnowMe.API.Domain.Entities;

public class Game
{
    public Guid Id { get; private set;}
    public string Name { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public List<User> Players { get; private set; }
    public List<Question> Questions { get; private set; } = new List<Question>();

    public Guid CurrentQuestionId { get; private set; }
    public Guid CreatedByUser { get; private set; }
    public GameStatus Status { get; private set; }


    public static Result<Game> Create(string name, User createdBy)
    {
        var errors = new List<ValidationError>();

        if (name.Length > 100)
        {
            errors.Add(new ValidationError
            {
                Message = "Name cannot be longer than 100 characters"
            });
        }

        if (errors.Count != 0)
        {
            return Result<Game>.Failure(errors);
        }

        var game = new Game
        {
            Id = Guid.NewGuid(),
            Name = name,
            CreatedByUser = createdBy.Id,
            Players = [createdBy],
            Status = GameStatus.Created
        };

        return Result<Game>.Success(game);
    }

    public Result<Game> AddPlayer(User player)
    {
        var errors = new List<ValidationError>();

        if (Players.Any(p => p.Id == player.Id))
        {
            errors.Add(new ValidationError
            {
                Message = "Cannot add duplicate player"
            });
        }

        if (errors.Count != 0)
        {
            return Result<Game>.Failure(errors);
        }

        Players.Add(player);

        //TODO Domain event that player added
        return Result<Game>.Success(this);
    }

    public void AddQuestion(Question question)
    {
        //TODO Domain event that question added
        Questions.Add(question);
    }

    public Result<Game> StartGame()
    {
        var errors = new List<ValidationError>();

        if (Players.Count == 1)
        {
            errors.Add(new ValidationError
            {
                Message = "Cannot start game with only one player"
            });
        }

        if (Questions.Count <= 1)
        {
            errors.Add(new ValidationError
            {
                Message = "At least two questions required to start the game"
            });
        }

        if (errors.Count != 0)
        {
            return Result<Game>.Failure(errors);
        }

        Status = GameStatus.Started;
        CurrentQuestionId = Questions.OrderBy(q => q.Id).First().Id;


        //TODO Domain event that game started
        return Result<Game>.Success(this);
    }
}
