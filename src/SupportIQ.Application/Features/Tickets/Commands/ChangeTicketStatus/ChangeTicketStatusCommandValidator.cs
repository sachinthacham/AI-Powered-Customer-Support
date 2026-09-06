using FluentValidation;

namespace SupportIQ.Application.Features.Tickets.Commands.ChangeTicketStatus;

public class ChangeTicketStatusCommandValidator : AbstractValidator<ChangeTicketStatusCommand>
{
    public ChangeTicketStatusCommandValidator()
    {
        RuleFor(x => x.TicketId).NotEmpty();
        RuleFor(x => x.Status).IsInEnum();
    }
}
