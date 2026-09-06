using FluentValidation;

namespace SupportIQ.Application.Features.Tickets.Commands.EscalateTicket;

public class EscalateTicketCommandValidator : AbstractValidator<EscalateTicketCommand>
{
    public EscalateTicketCommandValidator()
    {
        RuleFor(x => x.TicketId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}
