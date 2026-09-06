using MediatR;
using Microsoft.Extensions.Logging;
using SupportIQ.Application.Abstractions;
using SupportIQ.Application.Common.Exceptions;
using SupportIQ.Domain.Entities;

namespace SupportIQ.Application.Features.Tickets.Commands.DeleteTicket;

public class DeleteTicketCommandHandler : IRequestHandler<DeleteTicketCommand>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IApplicationDbContext _context;
    private readonly ILogger<DeleteTicketCommandHandler> _logger;

    public DeleteTicketCommandHandler(
        ITicketRepository ticketRepository,
        IApplicationDbContext context,
        ILogger<DeleteTicketCommandHandler> logger)
    {
        _ticketRepository = ticketRepository;
        _context = context;
        _logger = logger;
    }

    public async Task Handle(DeleteTicketCommand request, CancellationToken cancellationToken)
    {
        var ticket = await _ticketRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(SupportTicket), request.Id);

        _ticketRepository.Remove(ticket);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Ticket {TicketId} deleted", ticket.Id);
    }
}
