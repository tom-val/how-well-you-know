using KnowMe.API.Domain.Validation;

namespace KnowMe.API.Domain.Entities;

public class Question
{
    public Guid Id { get; private set;}
    public string Text { get; private set; }
    public bool MultipleAnswers { get; private set; }
    public Guid CreatedByUser { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public List<QuestionVariant> AnswerVariants { get; private set; } = new List<QuestionVariant>();
    public Game Game { get; private set; }
    public Guid GameId { get; private set; }
    public List<QuestionUserChoice> UserChoices { get; private set; } = new List<QuestionUserChoice>();
    public List<QuestionUserGuess> UserGuesses { get; private set; } = new List<QuestionUserGuess>();
    public bool Answered { get; private set; }

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

        //If everyone answered, calculate question result
        var peopleCount = Game.Players.Count;
        var guessCount = peopleCount * (peopleCount - 1);

        if (UserGuesses.Count != guessCount)
        {
            return Result<Question>.Success(this);
        }

        Answered = true;
        //Raise question answered domain event

        return Result<Question>.Success(this);
    }


    //Decrease one point for each incorrect answer
    private int CalculateMultipleAnswersScore(List<Guid> shouldNotBeSelected, List<Guid> shouldBeSelected)
    {
        var incorrectAnswersCount = shouldBeSelected.Count + shouldBeSelected.Count;
        var score = 3 - incorrectAnswersCount;
        return score < 0 ? 0 : score;
    }

    private int CalculateNonMultipleAnswersScore(List<Guid> shouldNotBeSelected, List<Guid> shouldBeSelected)
    {
        var incorrectAnswersCount = shouldBeSelected.Count + shouldBeSelected.Count;
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

        var createdQuestionVariants = new List<QuestionVariant>();
        foreach (var questionVariant in variants.Select(variant => QuestionVariant.Create(variant.Value, variant.Key)))
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

        var question = new Question
        {
            Id = Guid.NewGuid(),
            Text = text,
            AnswerVariants = createdQuestionVariants,
            CreatedByUser = createdBy.Id,
            GameId = game.Id,
            MultipleAnswers = multipleAnswers
        };

        return Result<Question>.Success(question);
    }
}

public record UserGuessResult(Guid GuessingUser, Guid ChoiceUser, int Score, List<Guid> NotSelectedChoices, List<Guid> ShouldNotBeSelectedChoices);
public record UserResult(Guid UserId, int TotalScore, List<UserGuessResult> GuessResults);
