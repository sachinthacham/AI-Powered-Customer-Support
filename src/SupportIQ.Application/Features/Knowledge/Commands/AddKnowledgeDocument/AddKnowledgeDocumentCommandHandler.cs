using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SupportIQ.Application.Abstractions;
using SupportIQ.Application.Common.TextProcessing;
using SupportIQ.Application.DTOs;
using SupportIQ.Application.Common.Options;
using Microsoft.Extensions.Options;
using SupportIQ.Domain.Entities;

namespace SupportIQ.Application.Features.Knowledge.Commands.AddKnowledgeDocument;

/// <summary>
/// Ingests a knowledge document: chunk -> embed -> upsert into the vector store -> record
/// metadata in SQL. If a document with the same file name already exists with identical
/// content, ingestion is skipped entirely (see README "Cost Control" - we never pay to
/// re-embed text that hasn't changed). If the content changed, the old vectors are replaced.
/// </summary>
public class AddKnowledgeDocumentCommandHandler : IRequestHandler<AddKnowledgeDocumentCommand, KnowledgeDocumentDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IEmbeddingService _embeddingService;
    private readonly IVectorStore _vectorStore;
    private readonly RagOptions _ragOptions;
    private readonly ILogger<AddKnowledgeDocumentCommandHandler> _logger;

    public AddKnowledgeDocumentCommandHandler(
        IApplicationDbContext context,
        IEmbeddingService embeddingService,
        IVectorStore vectorStore,
        IOptions<RagOptions> ragOptions,
        ILogger<AddKnowledgeDocumentCommandHandler> logger)
    {
        _context = context;
        _embeddingService = embeddingService;
        _vectorStore = vectorStore;
        _ragOptions = ragOptions.Value;
        _logger = logger;
    }

    public async Task<KnowledgeDocumentDto> Handle(AddKnowledgeDocumentCommand request, CancellationToken cancellationToken)
    {
        var existing = await _context.KnowledgeDocuments
            .FirstOrDefaultAsync(d => d.FileName == request.FileName, cancellationToken);

        if (existing is not null && existing.Content == request.Content)
        {
            _logger.LogInformation("Knowledge document {FileName} unchanged - skipping re-ingestion", request.FileName);
            return ToDto(existing);
        }

        var chunks = TextChunker.Chunk(request.Content, _ragOptions.ChunkSize, _ragOptions.ChunkOverlap);
        if (chunks.Count == 0)
            throw new ArgumentException("Document content produced no chunks after processing.");

        var embeddings = await _embeddingService.EmbedBatchAsync(chunks, cancellationToken);

        var document = existing ?? KnowledgeDocument.Create(request.FileName, request.Title, request.Content);
        if (existing is not null)
        {
            existing.Update(request.Title, request.Content);
        }
        else
        {
            _context.KnowledgeDocuments.Add(document);
        }

        var vectorChunks = chunks
            .Select((text, index) => new VectorChunk(index, text, embeddings[index]))
            .ToList();

        await _vectorStore.EnsureCollectionExistsAsync(cancellationToken);
        await _vectorStore.UpsertChunksAsync(document.Id, document.Title, vectorChunks, cancellationToken);

        document.SetChunkCount(chunks.Count);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Knowledge document {FileName} ingested with {ChunkCount} chunks", document.FileName, chunks.Count);

        return ToDto(document);
    }

    private static KnowledgeDocumentDto ToDto(KnowledgeDocument document) =>
        new(document.Id, document.FileName, document.Title, document.ChunkCount, document.CreatedAt, document.UpdatedAt);
}
