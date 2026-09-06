using MediatR;
using Microsoft.Extensions.Logging;
using SupportIQ.Application.Abstractions;
using SupportIQ.Application.Common.Mappings;
using SupportIQ.Application.DTOs;
using SupportIQ.Domain.Entities;

namespace SupportIQ.Application.Features.Tickets.Commands.CreateTicket;

public class CreateTicketCommandHandler : IRequestHandler<CreateTicketCommand, TicketDto>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<CreateTicketCommandHandler> _logger;

    public CreateTicketCommandHandler(
        ITicketRepository ticketRepository,
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        ILogger<CreateTicketCommandHandler> logger)
    {
        _ticketRepository = ticketRepository;
        _context = context;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<TicketDto> Handle(CreateTicketCommand request, CancellationToken cancellationToken)
    {
        var ticket = SupportTicket.Create(request.Title, request.Description, request.CustomerEmail);

        _ticketRepository.Add(ticket);
        _context.AuditLogs.Add(AuditLog.Create(
            nameof(SupportTicket), ticket.Id.ToString(), "Created", _currentUser.Email ?? "system"));

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Ticket {TicketId} created", ticket.Id);

        return ticket.ToDto();
    }
}
