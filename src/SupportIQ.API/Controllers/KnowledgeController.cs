using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportIQ.Application.DTOs;
using SupportIQ.Application.Features.Knowledge.Commands.AddKnowledgeDocument;
using SupportIQ.Application.Features.Knowledge.Commands.DeleteKnowledgeDocument;
using SupportIQ.Application.Features.Knowledge.Queries.GetKnowledgeDocuments;

namespace SupportIQ.API.Controllers;

/// <summary>Manages the company knowledge base documents that back the RAG pipeline.</summary>
[ApiController]
[Authorize]
[Route("api/knowledge/documents")]
[Produces("application/json")]
public class KnowledgeController : ControllerBase
{
    private readonly ISender _mediator;

    public KnowledgeController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Ingests a knowledge document: chunks the text, generates embeddings, and stores the
    /// vectors in Qdrant. Re-uploading an unchanged file (same fileName + content) is a no-op.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(KnowledgeDocumentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<KnowledgeDocumentDto>> Create(AddKnowledgeDocumentCommand command, CancellationToken cancellationToken)
    {
        var document = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetAll), new { }, document);
    }

    /// <summary>Lists all ingested knowledge documents (metadata only - chunk text lives in Qdrant).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<KnowledgeDocumentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<KnowledgeDocumentDto>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(new GetKnowledgeDocumentsQuery(), cancellationToken));
    }

    /// <summary>Deletes a knowledge document and its vectors from Qdrant.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteKnowledgeDocumentCommand(id), cancellationToken);
        return NoContent();
    }
}
