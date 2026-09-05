using MediatR;
using SupportIQ.Application.DTOs;

namespace SupportIQ.Application.Features.Ai.Commands.AskQuestion;

public record AskQuestionCommand(string Question) : IRequest<RagAnswerDto>;
