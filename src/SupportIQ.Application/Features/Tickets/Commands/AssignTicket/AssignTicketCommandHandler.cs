using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SupportIQ.Application.Abstractions;
using SupportIQ.Application.Common.Exceptions;
using SupportIQ.Application.Common.Mappings;
using SupportIQ.Application.DTOs;
using SupportIQ.Domain.Entities;

namespace SupportIQ.Application.Features.Tickets.Commands.AssignTicket;

public class AssignTicketCommandHandler : IRequestHandler<AssignTicketCommand, TicketDto>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IApplicationDbContext _context;
    private readonly ILogger<AssignTicketCommandHandler> _logger;

    public AssignTicketCommandHandler(
        ITicketRepository ticketRepository,
        IApplicationDbContext context,
        ILogger<AssignTicketCommandHandler> logger)
    {
        _ticketRepository = ticketRepository;
        _context = context;
        _logger = logger;
    }

    public async Task<TicketDto> Handle(AssignTicketCommand request, CancellationToken cancellationToken)
    {
        var ticket = await _ticketRepository.GetByIdAsync(request.TicketId, cancellationToken)
            ?? throw new NotFoundException(nameof(SupportTicket), request.TicketId);

        var agentExists = await _context.Agents.AnyAsync(a => a.Id == request.AgentId, cancellationToken);
        if (!agentExists)
            throw new NotFoundException(nameof(SupportAgent), request.AgentId);

        ticket.AssignTo(request.AgentId);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Ticket {TicketId} assigned to agent {AgentId}", ticket.Id, request.AgentId);

        var refreshed = await _ticketRepository.GetByIdAsync(ticket.Id, cancellationToken);
        return refreshed!.ToDto();
    }
}
