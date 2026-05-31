using FluentValidation;

namespace KnowMe.API.Features.Games;

public class CreateGameRequestValidator : AbstractValidator<CreateGameRequest>
{
    public CreateGameRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);
    }
}

public class AddQuestionRequestValidator : AbstractValidator<AddQuestionRequest>
{
    public AddQuestionRequestValidator()
    {
        RuleFor(x => x.Text)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Variants)
            .NotEmpty()
            .Must(v => v.Keys.All(k => k.Length == 1))
            .WithMessage("Each variant notation must be a single character.");
    }
}

public class RecordChoiceRequestValidator : AbstractValidator<RecordChoiceRequest>
{
    public RecordChoiceRequestValidator()
    {
        RuleFor(x => x.VariantNotations).NotEmpty();
    }
}

public class RecordGuessRequestValidator : AbstractValidator<RecordGuessRequest>
{
    public RecordGuessRequestValidator()
    {
        RuleFor(x => x.ChoiceUserId).NotEmpty();
        RuleFor(x => x.VariantNotations).NotEmpty();
    }
}
