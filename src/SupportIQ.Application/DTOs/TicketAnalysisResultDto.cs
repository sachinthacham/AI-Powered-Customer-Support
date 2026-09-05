namespace SupportIQ.Application.DTOs;

/// <summary>The shape returned by POST /api/tickets/{id}/analyze and /api/ai/analyze-ticket.</summary>
public record TicketAnalysisResultDto(
    Guid TicketId,
    string Category,
    string Priority,
    string Sentiment,
    string Summary,
    IReadOnlyList<string> Tags,
    string SuggestedResponse,
    double Confidence,
    bool Escalated);
