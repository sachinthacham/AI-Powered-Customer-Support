using SupportIQ.Application.DTOs;
using SupportIQ.Domain.Entities;

namespace SupportIQ.Application.Common.Mappings;

public static class TicketMappingExtensions
{
    public static TicketDto ToDto(this SupportTicket ticket)
    {
        return new TicketDto(
            ticket.Id,
            ticket.Title,
            ticket.Description,
            ticket.CustomerEmail,
            ticket.Category?.ToString(),
            ticket.Priority?.ToString(),
            ticket.Sentiment?.ToString(),
            ticket.Status.ToString(),
            ticket.AssignedAgentId,
            ticket.AssignedAgent?.Name,
            ticket.Summary,
            ticket.SuggestedResponse,
            ticket.AiConfidence,
            ticket.EscalationReason,
            ticket.Tags.Select(t => t.Value).ToList(),
            ticket.CreatedAt,
            ticket.UpdatedAt);
    }
}
