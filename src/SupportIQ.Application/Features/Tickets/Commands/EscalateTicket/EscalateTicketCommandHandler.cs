using MediatR;
using Microsoft.Extensions.Logging;
using SupportIQ.Application.Abstractions;
using SupportIQ.Application.Common.Exceptions;
using SupportIQ.Application.Common.Mappings;
using SupportIQ.Application.DTOs;
using SupportIQ.Domain.Entities;

namespace SupportIQ.Application.Features.Tickets.Commands.EscalateTicket;

public class EscalateTicketCommandHandler : IRequestHandler<EscalateTicketCommand, TicketDto>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<EscalateTicketCommandHandler> _logger;

    public EscalateTicketCommandHandler(
        ITicketRepository ticketRepository,
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        ILogger<EscalateTicketCommandHandler> logger)
    {
        _ticketRepository = ticketRepository;
        _context = context;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<TicketDto> Handle(EscalateTicketCommand request, CancellationToken cancellationToken)
    {
        var ticket = await _ticketRepository.GetByIdAsync(request.TicketId, cancellationToken)
            ?? throw new NotFoundException(nameof(SupportTicket), request.TicketId);

        ticket.Escalate(request.Reason);

        _context.AuditLogs.Add(AuditLog.Create(
            nameof(SupportTicket), ticket.Id.ToString(), "Escalated", _currentUser.Email ?? "system", request.Reason));

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Ticket {TicketId} escalated", ticket.Id);

        return ticket.ToDto();
    }
}
