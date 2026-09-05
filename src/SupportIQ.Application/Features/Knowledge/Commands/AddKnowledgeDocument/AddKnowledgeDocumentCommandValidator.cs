using FluentValidation;

namespace SupportIQ.Application.Features.Knowledge.Commands.AddKnowledgeDocument;

public class AddKnowledgeDocumentCommandValidator : AbstractValidator<AddKnowledgeDocumentCommand>
{
    public AddKnowledgeDocumentCommandValidator()
    {
        RuleFor(x => x.FileName).NotEmpty().MaximumLength(260);
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Content).NotEmpty().MinimumLength(20);
    }
}
