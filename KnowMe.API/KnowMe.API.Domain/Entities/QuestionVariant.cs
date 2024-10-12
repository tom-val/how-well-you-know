using KnowMe.API.Domain.Validation;

namespace KnowMe.API.Domain.Entities;

public class QuestionVariant
{
    public Guid Id { get; private set;}
    public string Text { get; private set;}
    public char Notation { get; private set;}
    public Question Question { get; private set;}
    public Guid QuestionId { get; private set;}

    public static Result<QuestionVariant> Create(Question question, string text, char notation)
    {
        var errors = new List<ValidationError>();

        if (text.Length > 100)
        {
            errors.Add(new ValidationError
            {
                Message = "Answer text cannot be longer than 100 characters"
            });
        }

        if (errors.Count != 0)
        {
            return Result<QuestionVariant>.Failure(errors);
        }

        var variant = new QuestionVariant
        {
            Id = Guid.NewGuid(),
            Text = text,
            Notation = notation,
            Question = question,
            QuestionId = question.Id
        };

        return Result<QuestionVariant>.Success(variant);
    }

    public void AssignToQuestion(Question question)
    {
        QuestionId = question.Id;
    }
}
