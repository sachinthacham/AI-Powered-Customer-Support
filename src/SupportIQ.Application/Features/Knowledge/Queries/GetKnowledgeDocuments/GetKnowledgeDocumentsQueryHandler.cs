using MediatR;
using Microsoft.EntityFrameworkCore;
using SupportIQ.Application.Abstractions;
using SupportIQ.Application.DTOs;

namespace SupportIQ.Application.Features.Knowledge.Queries.GetKnowledgeDocuments;

public class GetKnowledgeDocumentsQueryHandler : IRequestHandler<GetKnowledgeDocumentsQuery, IReadOnlyList<KnowledgeDocumentDto>>
{
    private readonly IApplicationDbContext _context;

    public GetKnowledgeDocumentsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<KnowledgeDocumentDto>> Handle(GetKnowledgeDocumentsQuery request, CancellationToken cancellationToken)
    {
        return await _context.KnowledgeDocuments
            .OrderByDescending(d => d.CreatedAt)
            .Select(d => new KnowledgeDocumentDto(d.Id, d.FileName, d.Title, d.ChunkCount, d.CreatedAt, d.UpdatedAt))
            .ToListAsync(cancellationToken);
    }
}
