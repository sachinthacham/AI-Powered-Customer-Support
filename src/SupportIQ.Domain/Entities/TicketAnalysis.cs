using SupportIQ.Domain.Enums;

namespace SupportIQ.Domain.Entities;

/// <summary>
/// An immutable, point-in-time record of an AI analysis run against a ticket.
/// The <see cref="SupportTicket"/> itself always reflects the *latest* analysis;
/// this table preserves history for audit and evaluation purposes.
/// </summary>
public class TicketAnalysis
{
    public Guid Id { get; private set; }
    public Guid TicketId { get; private set; }
    public TicketCategory Category { get; private set; }
    public TicketPriority Priority { get; private set; }
    public TicketSentiment Sentiment { get; private set; }
    public string Summary { get; private set; } = default!;
    public string SuggestedResponse { get; private set; } = default!;
    public string TagsCsv { get; private set; } = default!;
    public double Confidence { get; private set; }
    public bool WasEscalated { get; private set; }
    public string ModelUsed { get; private set; } = default!;
    public DateTime CreatedAt { get; private set; }

    public IReadOnlyList<string> Tags =>
        TagsCsv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    private TicketAnalysis()
    {
    }

    public static TicketAnalysis Create(
        Guid ticketId,
        TicketCategory category,
        TicketPriority priority,
        TicketSentiment sentiment,
        string summary,
        string suggestedResponse,
        IEnumerable<string> tags,
        double confidence,
        bool wasEscalated,
        string modelUsed)
    {
        return new TicketAnalysis
        {
            Id = Guid.NewGuid(),
            TicketId = ticketId,
            Category = category,
            Priority = priority,
            Sentiment = sentiment,
            Summary = summary,
            SuggestedResponse = suggestedResponse,
            TagsCsv = string.Join(",", tags),
            Confidence = confidence,
            WasEscalated = wasEscalated,
            ModelUsed = modelUsed,
            CreatedAt = DateTime.UtcNow
        };
    }
}
