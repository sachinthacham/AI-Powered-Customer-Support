using MediatR;
using SupportIQ.Application.DTOs;

namespace SupportIQ.Application.Features.Knowledge.Queries.GetKnowledgeDocuments;

public record GetKnowledgeDocumentsQuery : IRequest<IReadOnlyList<KnowledgeDocumentDto>>;
