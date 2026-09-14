using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SupportIQ.Domain.Entities;

namespace SupportIQ.Infrastructure.Persistence.Configurations;

public class TicketAnalysisConfiguration : IEntityTypeConfiguration<TicketAnalysis>
{
    public void Configure(EntityTypeBuilder<TicketAnalysis> builder)
    {
        builder.ToTable("TicketAnalyses");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedNever();

        builder.Property(a => a.Category).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(a => a.Priority).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(a => a.Sentiment).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(a => a.Summary).HasMaxLength(2000).IsRequired();
        builder.Property(a => a.SuggestedResponse).HasMaxLength(4000).IsRequired();
        builder.Property(a => a.TagsCsv).HasMaxLength(500).IsRequired();
        builder.Property(a => a.ModelUsed).HasMaxLength(100).IsRequired();
        builder.Property(a => a.CreatedAt).IsRequired();

        builder.HasIndex(a => new { a.TicketId, a.CreatedAt });
    }
}
