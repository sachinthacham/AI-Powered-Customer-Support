using SupportIQ.Domain.Enums;
using SupportIQ.Domain.Exceptions;

namespace SupportIQ.Domain.Entities;

/// <summary>
/// The SupportTicket aggregate root. All AI-derived fields (category, priority,
/// sentiment, summary, suggested response, confidence, tags) start unset and are
/// only ever populated through <see cref="ApplyAiAnalysis"/>, keeping the "has this
/// ticket been analyzed yet" question answerable from state alone.
/// </summary>
public class SupportTicket
{
    private readonly List<TicketTag> _tags = new();
    private readonly List<TicketAnalysis> _analyses = new();

    public Guid Id { get; private set; }
    public string Title { get; private set; } = default!;
    public string Description { get; private set; } = default!;
    public string CustomerEmail { get; private set; } = default!;
    public TicketCategory? Category { get; private set; }
    public TicketPriority? Priority { get; private set; }
    public TicketSentiment? Sentiment { get; private set; }
    public TicketStatus Status { get; private set; }
    public Guid? AssignedAgentId { get; private set; }
    public string? Summary { get; private set; }
    public string? SuggestedResponse { get; private set; }
    public double? AiConfidence { get; private set; }
    public string? EscalationReason { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public SupportAgent? AssignedAgent { get; private set; }
    public IReadOnlyCollection<TicketTag> Tags => _tags.AsReadOnly();
    public IReadOnlyCollection<TicketAnalysis> Analyses => _analyses.AsReadOnly();

    private SupportTicket()
    {
    }

    public static SupportTicket Create(string title, string description, string customerEmail)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required.", nameof(title));
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description is required.", nameof(description));
        if (string.IsNullOrWhiteSpace(customerEmail))
            throw new ArgumentException("Customer email is required.", nameof(customerEmail));

        var now = DateTime.UtcNow;
        return new SupportTicket
        {
            Id = Guid.NewGuid(),
            Title = title.Trim(),
            Description = description.Trim(),
            CustomerEmail = customerEmail.Trim(),
            Status = TicketStatus.Open,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public void UpdateDetails(string? title, string? description)
    {
        EnsureNotClosed("update");

        if (!string.IsNullOrWhiteSpace(title))
            Title = title.Trim();

        if (!string.IsNullOrWhiteSpace(description))
            Description = description.Trim();

        Touch();
    }

    /// <summary>
    /// Assigns the ticket to an agent. A newly-opened ticket automatically moves
    /// to <see cref="TicketStatus.InProgress"/> once someone owns it.
    /// </summary>
    public void AssignTo(Guid agentId)
    {
        EnsureNotClosed("assign");

        AssignedAgentId = agentId;
        if (Status == TicketStatus.Open)
            Status = TicketStatus.InProgress;

        Touch();
    }

    public void ChangeStatus(TicketStatus newStatus)
    {
        if (Status == TicketStatus.Closed)
            throw new InvalidTicketStateException(
                $"Ticket {Id} is closed and its status can no longer change.");

        Status = newStatus;
        Touch();
    }

    /// <summary>
    /// Records the outcome of an AI analysis run as the ticket's current AI state.
    /// Does not decide escalation - that is an application-level policy decision
    /// (see the confidence threshold configuration) kept out of the domain model
    /// so it can be tuned without a code change.
    /// </summary>
    public void ApplyAiAnalysis(
        TicketCategory category,
        TicketPriority priority,
        TicketSentiment sentiment,
        string summary,
        string suggestedResponse,
        IEnumerable<string> tags,
        double confidence)
    {
        EnsureNotClosed("analyze");

        Category = category;
        Priority = priority;
        Sentiment = sentiment;
        Summary = summary;
        SuggestedResponse = suggestedResponse;
        AiConfidence = confidence;

        _tags.Clear();
        foreach (var tag in tags.Where(t => !string.IsNullOrWhiteSpace(t)).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            _tags.Add(TicketTag.Create(Id, tag));
        }

        Touch();
    }

    public void RecordAnalysis(TicketAnalysis analysis) => _analyses.Add(analysis);

    /// <summary>
    /// Updates only the suggested response, independent of a full <see cref="ApplyAiAnalysis"/>
    /// run - used by POST /generate-response so an agent can refresh the draft reply without
    /// paying for (or overwriting) a full re-classification.
    /// </summary>
    public void UpdateSuggestedResponse(string suggestedResponse)
    {
        EnsureNotClosed("update the suggested response for");

        if (string.IsNullOrWhiteSpace(suggestedResponse))
            throw new ArgumentException("Suggested response cannot be empty.", nameof(suggestedResponse));

        SuggestedResponse = suggestedResponse.Trim();
        Touch();
    }

    public void Escalate(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("An escalation reason is required.", nameof(reason));

        Status = TicketStatus.Escalated;
        EscalationReason = reason.Trim();
        Touch();
    }

    private void EnsureNotClosed(string action)
    {
        if (Status == TicketStatus.Closed)
            throw new InvalidTicketStateException($"Cannot {action} ticket {Id} because it is closed.");
    }

    private void Touch() => UpdatedAt = DateTime.UtcNow;
}
