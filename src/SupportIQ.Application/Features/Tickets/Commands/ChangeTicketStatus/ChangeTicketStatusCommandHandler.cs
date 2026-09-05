using MediatR;
using Microsoft.Extensions.Logging;
using SupportIQ.Application.Abstractions;
using SupportIQ.Application.Common.Exceptions;
using SupportIQ.Application.Common.Mappings;
using SupportIQ.Application.DTOs;
using SupportIQ.Domain.Entities;

namespace SupportIQ.Application.Features.Tickets.Commands.ChangeTicketStatus;

public class ChangeTicketStatusCommandHandler : IRequestHandler<ChangeTicketStatusCommand, TicketDto>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IApplicationDbContext _context;
    private readonly ILogger<ChangeTicketStatusCommandHandler> _logger;

    public ChangeTicketStatusCommandHandler(
        ITicketRepository ticketRepository,
        IApplicationDbContext context,
        ILogger<ChangeTicketStatusCommandHandler> logger)
    {
        _ticketRepository = ticketRepository;
        _context = context;
        _logger = logger;
    }

    public async Task<TicketDto> Handle(ChangeTicketStatusCommand request, CancellationToken cancellationToken)
    {
        var ticket = await _ticketRepository.GetByIdAsync(request.TicketId, cancellationToken)
            ?? throw new NotFoundException(nameof(SupportTicket), request.TicketId);

        ticket.ChangeStatus(request.Status);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Ticket {TicketId} status changed to {Status}", ticket.Id, request.Status);

        return ticket.ToDto();
    }
}
