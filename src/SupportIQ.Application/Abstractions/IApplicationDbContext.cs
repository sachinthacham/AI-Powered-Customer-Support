using Microsoft.EntityFrameworkCore;
using SupportIQ.Domain.Entities;

namespace SupportIQ.Application.Abstractions;

/// <summary>
/// The persistence seam the Application layer depends on. Exposing <see cref="DbSet{TEntity}"/>
/// directly (rather than one repository interface per table) is a deliberate choice: EF Core's
/// DbSet already *is* a repository/unit-of-work over its table, and SupportTickets are the only
/// aggregate complex enough to warrant a dedicated repository (see <see cref="ITicketRepository"/>).
/// Wrapping every other table in a near-identical interface would just be indirection with no
/// behavior behind it.
/// </summary>
public interface IApplicationDbContext
{
    DbSet<SupportTicket> Tickets { get; }
    DbSet<TicketTag> TicketTags { get; }
    DbSet<SupportAgent> Agents { get; }
    DbSet<TicketAnalysis> TicketAnalyses { get; }
    DbSet<KnowledgeDocument> KnowledgeDocuments { get; }
    DbSet<AuditLog> AuditLogs { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
