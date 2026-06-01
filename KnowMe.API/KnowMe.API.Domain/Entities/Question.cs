using KnowMe.API.Domain.Validation;

namespace KnowMe.API.Domain.Entities;

public class Question
{
    public Guid Id { get; private set;}
    public string Text { get; private set; }
    public bool MultipleAnswers { get; private set; }
    public int Order { get; internal set; }
    public Guid CreatedByUser { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public List<QuestionVariant> AnswerVariants { get; private set; } = new List<QuestionVariant>();
    public Game Game { get; private set; }
    public Guid GameId { get; private set; }
    public List<QuestionUserChoice> UserChoices { get; private set; } = new List<QuestionUserChoice>();
    public List<QuestionUserGuess> UserGuesses { get; private set; } = new List<QuestionUserGuess>();

    //This looks ok from domain perspective, but it should be done in transaction or some additional checks need to be performed
    //to make sure it's not stuck without answered question if multiple people answer at same time
    public bool Answered => UserGuesses.Count == Game.Players.Count * (Game.Players.Count - 1) &&
                            UserChoices.Count == Game.Players.Count;

    public Result<Question> RecordChoice(QuestionUserChoice choice)
    {
        var errors = new List<ValidationError>();
        if (UserChoices.Any(c => c.UserId == choice.UserId))
        {
            errors.Add(new ValidationError
            {
                Message = "User already made choice"
            });
        }

        if (errors.Count != 0)
        {
            return Result<Question>.Failure(errors);
        }

        UserChoices.Add(choice);

        return Result<Question>.Success(this);
    }

    public Result<Question> RecordGuess(QuestionUserGuess guess)
    {
        var errors = new List<ValidationError>();
        if (UserGuesses.Any(c => c.GuessingUserId == guess.GuessingUserId && c.ChoiceUserId == guess.ChoiceUserId))
        {
            errors.Add(new ValidationError
            {
                Message = "User already made guess for this user"
            });
        }

        if (errors.Count != 0)
        {
            return Result<Question>.Failure(errors);
        }

        UserGuesses.Add(guess);

        return Result<Question>.Success(this);
    }


    //Decrease one point for each incorrect answer
    private int CalculateMultipleAnswersScore(List<Guid> shouldNotBeSelected, List<Guid> shouldBeSelected)
    {
        var incorrectAnswersCount = shouldNotBeSelected.Count + shouldBeSelected.Count;
        var score = 3 - incorrectAnswersCount;
        return score < 0 ? 0 : score;
    }

    private int CalculateNonMultipleAnswersScore(List<Guid> shouldNotBeSelected, List<Guid> shouldBeSelected)
    {
        var incorrectAnswersCount = shouldNotBeSelected.Count + shouldBeSelected.Count;
        return incorrectAnswersCount == 0 ? 1 : 0;
    }

    private UserGuessResult GetUserGuessResult(QuestionUserGuess guess)
    {
        var choice = UserChoices.Single(c => c.UserId == guess.ChoiceUserId);
        var shouldNotBeSelected = guess.SelectedVariantsIds.Except(choice.SelectedVariantsIds).ToList();
        var shouldBeSelected = choice.SelectedVariantsIds.Except(guess.SelectedVariantsIds).ToList();

        return new UserGuessResult(
            guess.GuessingUserId,
            guess.ChoiceUserId,
            MultipleAnswers ? CalculateMultipleAnswersScore(shouldNotBeSelected, shouldBeSelected) : CalculateNonMultipleAnswersScore(shouldNotBeSelected, shouldBeSelected),
            shouldBeSelected,
            shouldNotBeSelected);
    }

    public Result<List<UserResult>> GetUserResults()
    {
        if (!Answered)
        {
            return Result<List<UserResult>>.Failure(
                [
                    new ValidationError
                    {
                        Message = "Cannot generate results until question fully answered"
                    }
                ]
            );
        }

        var userResults = new List<UserResult>();
        foreach (var person in Game.Players)
        {
            var userGuesses = UserGuesses.Where(g => g.GuessingUserId == person.Id);
            var guessResults = userGuesses.Select(GetUserGuessResult).ToList();
            userResults.Add(new UserResult(person.Id, guessResults.Sum(r => r.Score), guessResults));
        }

        return Result<List<UserResult>>.Success(userResults);
    }

    public static Result<Question> Create(string text, bool multipleAnswers, Dictionary<char, string> variants, User createdBy, Game game)
    {
        var errors = new List<ValidationError>();

        if (text.Length > 100)
        {
            errors.Add(new ValidationError
            {
                Message = "Question text cannot be longer than 100 characters"
            });
        }

        if (variants.Count <= 1)
        {
            errors.Add(new ValidationError
            {
                Message = "More than one possible question answer must be added"
            });
        }

        if (variants.Count > 20)
        {
            errors.Add(new ValidationError
            {
                Message = "No more than twenty possible question answer must be added"
            });
        }

        if (errors.Count != 0)
        {
            return Result<Question>.Failure(errors);
        }

        var question = new Question
        {
            Id = Guid.NewGuid(),
            Text = text,
            CreatedByUser = createdBy.Id,
            GameId = game.Id,
            Game = game,
            MultipleAnswers = multipleAnswers
        };

        var createdQuestionVariants = new List<QuestionVariant>();
        foreach (var questionVariant in variants.Select(variant => QuestionVariant.Create(question, variant.Value, variant.Key)))
        {
            if (!questionVariant.IsSuccess)
            {
                errors.AddRange(questionVariant.Errors!);
            }
            else
            {
                createdQuestionVariants.Add(questionVariant.Value);
            }
        }

        if (errors.Count != 0)
        {
            return Result<Question>.Failure(errors);
        }

        question.AnswerVariants = createdQuestionVariants;

        return Result<Question>.Success(question);
    }

    /// <summary>
    /// Reconstructs a question from persisted state. The owning <see cref="Game"/> is wired up
    /// separately via <see cref="AttachGame"/> once the aggregate is assembled.
    /// For use by the persistence layer only.
    /// </summary>
    internal static Question Rehydrate(
        Guid id,
        string text,
        bool multipleAnswers,
        int order,
        Guid createdByUser,
        DateTimeOffset createdAt,
        Guid gameId,
        List<QuestionVariant> answerVariants,
        List<QuestionUserChoice> userChoices,
        List<QuestionUserGuess> userGuesses) =>
        new()
        {
            Id = id,
            Text = text,
            MultipleAnswers = multipleAnswers,
            Order = order,
            CreatedByUser = createdByUser,
            CreatedAt = createdAt,
            GameId = gameId,
            AnswerVariants = answerVariants,
            UserChoices = userChoices,
            UserGuesses = userGuesses
        };

    /// <summary>
    /// Wires the owning game back-reference after the aggregate has been reconstructed,
    /// so <see cref="Answered"/> can read the player count. Persistence layer only.
    /// </summary>
    internal void AttachGame(Game game)
    {
        Game = game;
        GameId = game.Id;
    }
}

public record UserGuessResult(Guid GuessingUser, Guid ChoiceUser, int Score, List<Guid> NotSelectedChoices, List<Guid> ShouldNotBeSelectedChoices);
public record UserResult(Guid UserId, int TotalScore, List<UserGuessResult> GuessResults);
