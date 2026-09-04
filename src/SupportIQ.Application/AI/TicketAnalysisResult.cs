using SupportIQ.Domain.Enums;

namespace SupportIQ.Application.AI;

/// <summary>
/// The validated, strongly-typed result of an AI ticket analysis. By the time Application
/// code sees this, the raw AI JSON has already been parsed and every enum value confirmed
/// valid by the Infrastructure implementation - handlers never touch raw AI output.
/// </summary>
public record TicketAnalysisResult(
    TicketCategory Category,
    TicketPriority Priority,
    TicketSentiment Sentiment,
    string Summary,
    IReadOnlyList<string> Tags,
    string SuggestedResponse,
    double Confidence,
    string ModelUsed);
