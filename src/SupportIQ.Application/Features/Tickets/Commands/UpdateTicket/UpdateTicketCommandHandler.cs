using MediatR;
using Microsoft.Extensions.Logging;
using SupportIQ.Application.Abstractions;
using SupportIQ.Application.Common.Exceptions;
using SupportIQ.Application.Common.Mappings;
using SupportIQ.Application.DTOs;
using SupportIQ.Domain.Entities;

namespace SupportIQ.Application.Features.Tickets.Commands.UpdateTicket;

public class UpdateTicketCommandHandler : IRequestHandler<UpdateTicketCommand, TicketDto>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IApplicationDbContext _context;
    private readonly ILogger<UpdateTicketCommandHandler> _logger;

    public UpdateTicketCommandHandler(
        ITicketRepository ticketRepository,
        IApplicationDbContext context,
        ILogger<UpdateTicketCommandHandler> logger)
    {
        _ticketRepository = ticketRepository;
        _context = context;
        _logger = logger;
    }

    public async Task<TicketDto> Handle(UpdateTicketCommand request, CancellationToken cancellationToken)
    {
        var ticket = await _ticketRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(SupportTicket), request.Id);

        ticket.UpdateDetails(request.Title, request.Description);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Ticket {TicketId} updated", ticket.Id);

        return ticket.ToDto();
    }
}
