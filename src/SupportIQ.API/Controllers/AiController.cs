using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportIQ.Application.DTOs;
using SupportIQ.Application.Features.Ai.Commands.AskQuestion;
using SupportIQ.Application.Features.Tickets.Commands.AnalyzeTicket;

namespace SupportIQ.API.Controllers;

/// <summary>Provider-agnostic AI entry points that aren't tied to a single ticket's URL.</summary>
[ApiController]
[Authorize]
[Route("api/ai")]
[Produces("application/json")]
public class AiController : ControllerBase
{
    private readonly ISender _mediator;

    public AiController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Equivalent to POST /api/tickets/{id}/analyze, addressed by ticket id in the body.</summary>
    [HttpPost("analyze-ticket")]
    [ProducesResponseType(typeof(TicketAnalysisResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<ActionResult<TicketAnalysisResultDto>> AnalyzeTicket(AnalyzeTicketRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(new AnalyzeTicketCommand(request.TicketId), cancellationToken));
    }

    /// <summary>
    /// Answers a question using retrieval-augmented generation over the knowledge base. Returns
    /// a low-confidence fallback answer (with no sources) when nothing relevant enough is found,
    /// instead of letting the model guess.
    /// </summary>
    [HttpPost("ask")]
    [ProducesResponseType(typeof(RagAnswerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<ActionResult<RagAnswerDto>> Ask(AskQuestionCommand command, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(command, cancellationToken));
    }
}

public record AnalyzeTicketRequest(Guid TicketId);
