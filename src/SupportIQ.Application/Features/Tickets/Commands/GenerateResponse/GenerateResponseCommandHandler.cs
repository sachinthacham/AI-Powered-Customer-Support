using MediatR;
using SupportIQ.Application.Abstractions;
using SupportIQ.Application.Common.Exceptions;
using SupportIQ.Domain.Entities;

namespace SupportIQ.Application.Features.Tickets.Commands.GenerateResponse;

public class GenerateResponseCommandHandler : IRequestHandler<GenerateResponseCommand, string>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly ITicketAiService _aiService;
    private readonly IApplicationDbContext _context;

    public GenerateResponseCommandHandler(
        ITicketRepository ticketRepository,
        ITicketAiService aiService,
        IApplicationDbContext context)
    {
        _ticketRepository = ticketRepository;
        _aiService = aiService;
        _context = context;
    }

    public async Task<string> Handle(GenerateResponseCommand request, CancellationToken cancellationToken)
    {
        var ticket = await _ticketRepository.GetByIdAsync(request.TicketId, cancellationToken)
            ?? throw new NotFoundException(nameof(SupportTicket), request.TicketId);

        var response = await _aiService.GenerateResponseAsync(ticket, cancellationToken);

        ticket.UpdateSuggestedResponse(response);
        await _context.SaveChangesAsync(cancellationToken);

        return response;
    }
}
