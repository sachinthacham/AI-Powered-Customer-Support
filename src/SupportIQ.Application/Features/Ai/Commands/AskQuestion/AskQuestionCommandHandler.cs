using MediatR;
using SupportIQ.Application.Abstractions;
using SupportIQ.Application.DTOs;

namespace SupportIQ.Application.Features.Ai.Commands.AskQuestion;

public class AskQuestionCommandHandler : IRequestHandler<AskQuestionCommand, RagAnswerDto>
{
    private readonly IRagService _ragService;

    public AskQuestionCommandHandler(IRagService ragService)
    {
        _ragService = ragService;
    }

    public async Task<RagAnswerDto> Handle(AskQuestionCommand request, CancellationToken cancellationToken)
    {
        var answer = await _ragService.AskAsync(request.Question, cancellationToken);

        return new RagAnswerDto(
            answer.Answer,
            answer.Confidence,
            answer.Sources.Select(s => new RagSourceDto(s.Document, s.Chunk, s.Relevance)).ToList());
    }
}
