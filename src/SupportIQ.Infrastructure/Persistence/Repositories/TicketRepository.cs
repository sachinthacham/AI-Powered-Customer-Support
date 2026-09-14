using Microsoft.EntityFrameworkCore;
using SupportIQ.Application.Abstractions;
using SupportIQ.Domain.Entities;
using SupportIQ.Domain.Enums;

namespace SupportIQ.Infrastructure.Persistence.Repositories;

public class TicketRepository : ITicketRepository
{
    private readonly SupportIqDbContext _context;

    public TicketRepository(SupportIqDbContext context)
    {
        _context = context;
    }

    public async Task<SupportTicket?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Tickets
            .Include(t => t.Tags)
            .Include(t => t.AssignedAgent)
            .Include(t => t.Analyses.OrderByDescending(a => a.CreatedAt))
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<(IReadOnlyList<SupportTicket> Items, int TotalCount)> SearchAsync(
        TicketStatus? status,
        TicketCategory? category,
        TicketPriority? priority,
        Guid? assignedAgentId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Tickets
            .Include(t => t.Tags)
            .Include(t => t.AssignedAgent)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(t => t.Status == status.Value);

        if (category.HasValue)
            query = query.Where(t => t.Category == category.Value);

        if (priority.HasValue)
            query = query.Where(t => t.Priority == priority.Value);

        if (assignedAgentId.HasValue)
            query = query.Where(t => t.AssignedAgentId == assignedAgentId.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public void Add(SupportTicket ticket) => _context.Tickets.Add(ticket);

    public void Remove(SupportTicket ticket) => _context.Tickets.Remove(ticket);
}
