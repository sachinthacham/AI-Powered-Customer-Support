using Microsoft.EntityFrameworkCore;
using SupportIQ.Application.Abstractions;
using SupportIQ.Domain.Entities;

namespace SupportIQ.Infrastructure.Persistence;

public class SupportIqDbContext : DbContext, IApplicationDbContext
{
    public SupportIqDbContext(DbContextOptions<SupportIqDbContext> options) : base(options)
    {
    }

    public DbSet<SupportTicket> Tickets => Set<SupportTicket>();
    public DbSet<TicketTag> TicketTags => Set<TicketTag>();
    public DbSet<SupportAgent> Agents => Set<SupportAgent>();
    public DbSet<TicketAnalysis> TicketAnalyses => Set<TicketAnalysis>();
    public DbSet<KnowledgeDocument> KnowledgeDocuments => Set<KnowledgeDocument>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SupportIqDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
