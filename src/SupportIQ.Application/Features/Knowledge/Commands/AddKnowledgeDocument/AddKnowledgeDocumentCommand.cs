using MediatR;
using SupportIQ.Application.DTOs;

namespace SupportIQ.Application.Features.Knowledge.Commands.AddKnowledgeDocument;

public record AddKnowledgeDocumentCommand(string FileName, string Title, string Content) : IRequest<KnowledgeDocumentDto>;
