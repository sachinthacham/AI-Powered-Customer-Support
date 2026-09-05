using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SupportIQ.Application.Abstractions;
using SupportIQ.Application.Common.Exceptions;
using SupportIQ.Domain.Entities;

namespace SupportIQ.Application.Features.Knowledge.Commands.DeleteKnowledgeDocument;

public class DeleteKnowledgeDocumentCommandHandler : IRequestHandler<DeleteKnowledgeDocumentCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IVectorStore _vectorStore;
    private readonly ILogger<DeleteKnowledgeDocumentCommandHandler> _logger;

    public DeleteKnowledgeDocumentCommandHandler(
        IApplicationDbContext context,
        IVectorStore vectorStore,
        ILogger<DeleteKnowledgeDocumentCommandHandler> logger)
    {
        _context = context;
        _vectorStore = vectorStore;
        _logger = logger;
    }

    public async Task Handle(DeleteKnowledgeDocumentCommand request, CancellationToken cancellationToken)
    {
        var document = await _context.KnowledgeDocuments
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(KnowledgeDocument), request.Id);

        await _vectorStore.DeleteDocumentAsync(document.Id, cancellationToken);

        _context.KnowledgeDocuments.Remove(document);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Knowledge document {FileName} deleted", document.FileName);
    }
}
