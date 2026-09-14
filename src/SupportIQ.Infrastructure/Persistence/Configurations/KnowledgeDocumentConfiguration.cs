using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SupportIQ.Domain.Entities;

namespace SupportIQ.Infrastructure.Persistence.Configurations;

public class KnowledgeDocumentConfiguration : IEntityTypeConfiguration<KnowledgeDocument>
{
    public void Configure(EntityTypeBuilder<KnowledgeDocument> builder)
    {
        builder.ToTable("KnowledgeDocuments");

        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).ValueGeneratedNever();

        builder.Property(d => d.FileName).HasMaxLength(260).IsRequired();
        builder.Property(d => d.Title).HasMaxLength(300).IsRequired();
        builder.Property(d => d.Content).HasColumnType("nvarchar(max)").IsRequired();
        builder.Property(d => d.ChunkCount).IsRequired();
        builder.Property(d => d.CreatedAt).IsRequired();
        builder.Property(d => d.UpdatedAt).IsRequired();

        builder.HasIndex(d => d.FileName).IsUnique();
    }
}
