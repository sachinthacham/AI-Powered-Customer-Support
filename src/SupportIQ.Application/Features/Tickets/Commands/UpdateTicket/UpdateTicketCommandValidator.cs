using FluentValidation;

namespace SupportIQ.Application.Features.Tickets.Commands.UpdateTicket;

public class UpdateTicketCommandValidator : AbstractValidator<UpdateTicketCommand>
{
    public UpdateTicketCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Title).MaximumLength(200).When(x => x.Title is not null);
        RuleFor(x => x.Description).MaximumLength(4000).When(x => x.Description is not null);
        RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.Title) || !string.IsNullOrWhiteSpace(x.Description))
            .WithMessage("At least one of Title or Description must be provided.");
    }
}
