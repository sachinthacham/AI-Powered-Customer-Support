using MediatR;
using SupportIQ.Application.Abstractions;
using SupportIQ.Application.Common.Mappings;
using SupportIQ.Application.DTOs;

namespace SupportIQ.Application.Features.Tickets.Queries.SearchTickets;

public class SearchTicketsQueryHandler : IRequestHandler<SearchTicketsQuery, PagedResult<TicketDto>>
{
    private readonly ITicketRepository _ticketRepository;

    public SearchTicketsQueryHandler(ITicketRepository ticketRepository)
    {
        _ticketRepository = ticketRepository;
    }

    public async Task<PagedResult<TicketDto>> Handle(SearchTicketsQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _ticketRepository.SearchAsync(
            request.Status,
            request.Category,
            request.Priority,
            request.AssignedAgentId,
            request.Page,
            request.PageSize,
            cancellationToken);

        return new PagedResult<TicketDto>(
            items.Select(t => t.ToDto()).ToList(),
            request.Page,
            request.PageSize,
            totalCount);
    }
}
