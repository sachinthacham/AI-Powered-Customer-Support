using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportIQ.Application.DTOs;
using SupportIQ.Application.Features.Tickets.Commands.AnalyzeTicket;
using SupportIQ.Application.Features.Tickets.Commands.AssignTicket;
using SupportIQ.Application.Features.Tickets.Commands.ChangeTicketStatus;
using SupportIQ.Application.Features.Tickets.Commands.CreateTicket;
using SupportIQ.Application.Features.Tickets.Commands.DeleteTicket;
using SupportIQ.Application.Features.Tickets.Commands.EscalateTicket;
using SupportIQ.Application.Features.Tickets.Commands.GenerateResponse;
using SupportIQ.Application.Features.Tickets.Commands.UpdateTicket;
using SupportIQ.Application.Features.Tickets.Queries.GetTicketById;
using SupportIQ.Application.Features.Tickets.Queries.SearchTickets;
using SupportIQ.Domain.Enums;

namespace SupportIQ.API.Controllers;

/// <summary>CRUD and lifecycle operations for support tickets.</summary>
[ApiController]
[Authorize]
[Route("api/tickets")]
[Produces("application/json")]
public class TicketsController : ControllerBase
{
    private readonly ISender _mediator;

    public TicketsController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Creates a new support ticket.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(TicketDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<TicketDto>> Create(CreateTicketCommand command, CancellationToken cancellationToken)
    {
        var ticket = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = ticket.Id }, ticket);
    }

    /// <summary>Searches tickets with optional filters, paginated.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<TicketDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<TicketDto>>> Search(
        [FromQuery] TicketStatus? status,
        [FromQuery] TicketCategory? category,
        [FromQuery] TicketPriority? priority,
        [FromQuery] Guid? assignedAgentId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new SearchTicketsQuery(status, category, priority, assignedAgentId, page, pageSize);
        return Ok(await _mediator.Send(query, cancellationToken));
    }

    /// <summary>Gets a single ticket by id, including its tags and latest AI analysis fields.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TicketDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TicketDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(new GetTicketByIdQuery(id), cancellationToken));
    }

    /// <summary>Updates a ticket's title and/or description.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(TicketDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TicketDto>> Update(Guid id, UpdateTicketRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateTicketCommand(id, request.Title, request.Description);
        return Ok(await _mediator.Send(command, cancellationToken));
    }

    /// <summary>Permanently deletes a ticket.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteTicketCommand(id), cancellationToken);
        return NoContent();
    }

    /// <summary>Assigns a ticket to a support agent.</summary>
    [HttpPost("{id:guid}/assign")]
    [ProducesResponseType(typeof(TicketDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TicketDto>> Assign(Guid id, AssignTicketRequest request, CancellationToken cancellationToken)
    {
        var command = new AssignTicketCommand(id, request.AgentId);
        return Ok(await _mediator.Send(command, cancellationToken));
    }

    /// <summary>Updates a ticket's status.</summary>
    [HttpPut("{id:guid}/status")]
    [ProducesResponseType(typeof(TicketDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TicketDto>> ChangeStatus(Guid id, ChangeTicketStatusRequest request, CancellationToken cancellationToken)
    {
        var command = new ChangeTicketStatusCommand(id, request.Status);
        return Ok(await _mediator.Send(command, cancellationToken));
    }

    /// <summary>Manually escalates a ticket to a human for review.</summary>
    [HttpPost("{id:guid}/escalate")]
    [ProducesResponseType(typeof(TicketDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TicketDto>> Escalate(Guid id, EscalateTicketRequest request, CancellationToken cancellationToken)
    {
        var command = new EscalateTicketCommand(id, request.Reason);
        return Ok(await _mediator.Send(command, cancellationToken));
    }

    /// <summary>
    /// Runs AI analysis on a ticket: classifies category/priority/sentiment, writes a summary
    /// and tags, drafts a suggested response, and auto-escalates if AI confidence is too low.
    /// </summary>
    [HttpPost("{id:guid}/analyze")]
    [ProducesResponseType(typeof(TicketAnalysisResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<ActionResult<TicketAnalysisResultDto>> Analyze(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(new AnalyzeTicketCommand(id), cancellationToken));
    }

    /// <summary>Drafts (or refreshes) a suggested customer-facing reply, without a full re-analysis.</summary>
    [HttpPost("{id:guid}/generate-response")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<ActionResult<string>> GenerateResponse(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(new GenerateResponseCommand(id), cancellationToken));
    }
}

public record UpdateTicketRequest(string? Title, string? Description);

public record AssignTicketRequest(Guid AgentId);

public record ChangeTicketStatusRequest(TicketStatus Status);

public record EscalateTicketRequest(string Reason);
