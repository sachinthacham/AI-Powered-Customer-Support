namespace SupportIQ.Application.DTOs;

public record TicketDto(
    Guid Id,
    string Title,
    string Description,
    string CustomerEmail,
    string? Category,
    string? Priority,
    string? Sentiment,
    string Status,
    Guid? AssignedAgentId,
    string? AssignedAgentName,
    string? Summary,
    string? SuggestedResponse,
    double? AiConfidence,
    string? EscalationReason,
    IReadOnlyList<string> Tags,
    DateTime CreatedAt,
    DateTime UpdatedAt);
