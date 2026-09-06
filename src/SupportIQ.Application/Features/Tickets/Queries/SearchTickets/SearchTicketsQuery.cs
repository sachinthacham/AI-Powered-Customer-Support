using MediatR;
using SupportIQ.Application.DTOs;
using SupportIQ.Domain.Enums;

namespace SupportIQ.Application.Features.Tickets.Queries.SearchTickets;

public record SearchTicketsQuery(
    TicketStatus? Status,
    TicketCategory? Category,
    TicketPriority? Priority,
    Guid? AssignedAgentId,
    int Page = 1,
    int PageSize = 20) : IRequest<PagedResult<TicketDto>>;
