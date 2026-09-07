namespace SupportIQ.Domain.Entities;

/// <summary>
/// An append-only record of a significant action taken on a domain entity,
/// written explicitly by application handlers (e.g. ticket created, AI analysis
/// applied, ticket escalated). Not a full event-sourcing log — just enough
/// history to answer "who/what changed this ticket, and when".
/// </summary>
public class AuditLog
{
    public Guid Id { get; private set; }
    public string EntityName { get; private set; } = default!;
    public string EntityId { get; private set; } = default!;
    public string Action { get; private set; } = default!;
    public string PerformedBy { get; private set; } = default!;
    public string? Details { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private AuditLog()
    {
    }

    public static AuditLog Create(string entityName, string entityId, string action, string performedBy, string? details = null)
    {
        return new AuditLog
        {
            Id = Guid.NewGuid(),
            EntityName = entityName,
            EntityId = entityId,
            Action = action,
            PerformedBy = performedBy,
            Details = details,
            CreatedAt = DateTime.UtcNow
        };
    }
}
