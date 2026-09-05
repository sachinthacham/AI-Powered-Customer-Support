using MediatR;

namespace SupportIQ.Application.Features.Knowledge.Commands.DeleteKnowledgeDocument;

public record DeleteKnowledgeDocumentCommand(Guid Id) : IRequest;
