using SupportIQ.Domain.Entities;
using SupportIQ.Domain.Enums;

namespace SupportIQ.Application.Abstractions;

/// <summary>
/// Dedicated repository for the <see cref="SupportTicket"/> aggregate. Unlike the other
/// tables (exposed via <see cref="IApplicationDbContext"/>), tickets are read with
/// non-trivial, repeated filtering/paging/include logic - which is exactly the kind of
/// behavior a repository earns its keep by encapsulating.
/// </summary>
public interface ITicketRepository
{
    Task<SupportTicket?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<SupportTicket> Items, int TotalCount)> SearchAsync(
        TicketStatus? status,
        TicketCategory? category,
        TicketPriority? priority,
        Guid? assignedAgentId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    void Add(SupportTicket ticket);

    void Remove(SupportTicket ticket);
}
