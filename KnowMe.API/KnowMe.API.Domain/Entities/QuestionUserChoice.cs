using KnowMe.API.Domain.Validation;

namespace KnowMe.API.Domain.Entities;

public class QuestionUserChoice
{
    public Guid Id { get; set; }
    public Guid UserId { get; private set; }
    public Guid QuestionId { get; private set; }
    public List<Guid> SelectedVariantsIds { get; private set; } = [];

    public static Result<QuestionUserChoice> Create(User user, Question question, List<QuestionVariant> selectedVariants)
    {
        var errors = new List<ValidationError>();
        if (selectedVariants.Any(v => v.QuestionId != question.Id))
        {
            errors.Add(new ValidationError
            {
                Message = "Selected variant must belong to question"
            });
        }

        if (!question.MultipleAnswers && selectedVariants.Count != 1)
        {
            errors.Add(new ValidationError
            {
                Message = "Can select only one variant when not multiple answers"
            });
        }

        if (errors.Count != 0)
        {
            return Result<QuestionUserChoice>.Failure(errors);
        }

        var choice = new QuestionUserChoice
        {
            Id = Guid.NewGuid(),
            QuestionId = question.Id,
            UserId = user.Id,
            SelectedVariantsIds = selectedVariants.Select(v => v.Id).ToList()
        };

        return Result<QuestionUserChoice>.Success(choice);
    }
}
