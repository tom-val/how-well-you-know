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
    public QuestionPhase CurrentQuestionPhase { get; private set; }


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

        if (Status != GameStatus.Created)
        {
            errors.Add(new ValidationError
            {
                Message = "Cannot add players after the game has started"
            });
        }

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

    public Result<Game> RecordChoice(User user, List<QuestionVariant> selectedVariants)
    {
        var stateError = EnsureAcceptingAnswers();
        if (stateError is not null)
        {
            return Result<Game>.Failure([stateError]);
        }

        if (Players.All(p => p.Id != user.Id))
        {
            return Result<Game>.Failure([new ValidationError
            {
                Message = "User is not a player in this game"
            }]);
        }

        var currentQuestion = Questions.First(q => q.Id == CurrentQuestionId);
        var choiceResult = QuestionUserChoice.Create(user, currentQuestion, selectedVariants);

        if (!choiceResult.IsSuccess)
        {
            return Result<Game>.Failure(choiceResult.Errors!);
        }

        var addChoiceResult = currentQuestion.RecordChoice(choiceResult.Value);

        if (!addChoiceResult.IsSuccess)
        {
            return Result<Game>.Failure(addChoiceResult.Errors!);
        }

        MoveToReviewIfCurrentQuestionAnswered();

        return Result<Game>.Success(this);
    }

    public Result<Game> RecordGuess(User guessingUser, User choiceUser, List<QuestionVariant> selectedVariants)
    {
        var stateError = EnsureAcceptingAnswers();
        if (stateError is not null)
        {
            return Result<Game>.Failure([stateError]);
        }

        var errors = new List<ValidationError>();

        if (guessingUser.Id == choiceUser.Id)
        {
            errors.Add(new ValidationError
            {
                Message = "User cannot guess their own answer"
            });
        }

        if (Players.All(p => p.Id != guessingUser.Id) || Players.All(p => p.Id != choiceUser.Id))
        {
            errors.Add(new ValidationError
            {
                Message = "Both users must be players in this game"
            });
        }

        if (errors.Count != 0)
        {
            return Result<Game>.Failure(errors);
        }

        var currentQuestion = Questions.First(q => q.Id == CurrentQuestionId);
        var guessResult = QuestionUserGuess.Create(guessingUser, choiceUser, currentQuestion, selectedVariants);

        if (!guessResult.IsSuccess)
        {
            return Result<Game>.Failure(guessResult.Errors!);
        }

        var addGuessResult = currentQuestion.RecordGuess(guessResult.Value);

        if (!addGuessResult.IsSuccess)
        {
            return Result<Game>.Failure(addGuessResult.Errors!);
        }

        MoveToReviewIfCurrentQuestionAnswered();

        return Result<Game>.Success(this);
    }

    private void MoveToReviewIfCurrentQuestionAnswered()
    {
        var currentQuestion = Questions.First(q => q.Id == CurrentQuestionId);

        if (currentQuestion.Answered)
        {
            CurrentQuestionPhase = QuestionPhase.Review;
            //TODO Domain event that question entered review
        }
    }

    public Result<Game> AdvanceToNextQuestion()
    {
        var errors = new List<ValidationError>();

        if (Status != GameStatus.Started)
        {
            errors.Add(new ValidationError
            {
                Message = "Game is not in progress"
            });
        }
        else if (CurrentQuestionPhase != QuestionPhase.Review)
        {
            errors.Add(new ValidationError
            {
                Message = "Cannot advance until the current question is in review"
            });
        }

        if (errors.Count != 0)
        {
            return Result<Game>.Failure(errors);
        }

        var newQuestion = Questions.OrderBy(q => q.Id).FirstOrDefault(q => !q.Answered);

        //If no new questions, game is finished
        if (newQuestion is null)
        {
            Status = GameStatus.Ended;
            //Final scores are available on demand via GetResults()
            //TODO Add game ended domain event
        }
        else
        {
            CurrentQuestionId = newQuestion.Id;
            CurrentQuestionPhase = QuestionPhase.Answering;
            //TODO Domain event that question advanced
        }

        return Result<Game>.Success(this);
    }

    /// <summary>
    /// Aggregates every answered question into a per-player total score and ranking.
    /// Works mid-game (counting only answered questions) and as the final result once ended.
    /// </summary>
    public GameResult GetResults()
    {
        var totals = Players.ToDictionary(p => p.Id, _ => 0);

        foreach (var question in Questions.Where(q => q.Answered))
        {
            foreach (var userResult in question.GetUserResults().Value)
            {
                if (totals.ContainsKey(userResult.UserId))
                {
                    totals[userResult.UserId] += userResult.TotalScore;
                }
            }
        }

        return GameResult.Create(Id, totals.Select(kvp => (kvp.Key, kvp.Value)));
    }

    private ValidationError? EnsureAcceptingAnswers()
    {
        if (Status != GameStatus.Started)
        {
            return new ValidationError
            {
                Message = "Game is not in progress"
            };
        }

        if (CurrentQuestionPhase != QuestionPhase.Answering)
        {
            return new ValidationError
            {
                Message = "Current question is not accepting answers"
            };
        }

        return null;
    }

    public Result<Game> AddQuestion(Question question)
    {
        if (Status != GameStatus.Created)
        {
            return Result<Game>.Failure([new ValidationError
            {
                Message = "Cannot add questions after the game has started"
            }]);
        }

        Questions.Add(question);

        //TODO Domain event that question added
        return Result<Game>.Success(this);
    }

    public Result<Game> StartGame()
    {
        var errors = new List<ValidationError>();

        if (Status != GameStatus.Created)
        {
            errors.Add(new ValidationError
            {
                Message = "Game has already been started"
            });
        }

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
        CurrentQuestionPhase = QuestionPhase.Answering;
        CurrentQuestionId = Questions.OrderBy(q => q.Id).First().Id;


        //TODO Domain event that game started
        return Result<Game>.Success(this);
    }

    /// <summary>
    /// Reconstructs a game aggregate from persisted state without running creation validation,
    /// wiring each question's owning-game back-reference. For use by the persistence layer only.
    /// </summary>
    internal static Game Rehydrate(
        Guid id,
        string name,
        DateTimeOffset createdAt,
        List<User> players,
        List<Question> questions,
        Guid currentQuestionId,
        Guid createdByUser,
        GameStatus status,
        QuestionPhase currentQuestionPhase)
    {
        var game = new Game
        {
            Id = id,
            Name = name,
            CreatedAt = createdAt,
            Players = players,
            Questions = questions,
            CurrentQuestionId = currentQuestionId,
            CreatedByUser = createdByUser,
            Status = status,
            CurrentQuestionPhase = currentQuestionPhase
        };

        foreach (var question in questions)
        {
            question.AttachGame(game);
        }

        return game;
    }
}
