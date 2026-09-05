using FluentValidation;

namespace SupportIQ.Application.Features.Ai.Commands.AskQuestion;

public class AskQuestionCommandValidator : AbstractValidator<AskQuestionCommand>
{
    public AskQuestionCommandValidator()
    {
        RuleFor(x => x.Question).NotEmpty().MaximumLength(1000);
    }
}
