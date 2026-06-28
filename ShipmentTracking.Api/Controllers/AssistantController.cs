using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShipmentTracking.Core.DTOs;
using ShipmentTracking.Core.Interfaces;
using ShipmentTracking.Infrastructure.Exceptions;

namespace ShipmentTracking.Api.Controllers;

[ApiController]
[Route("api/assistant")]
[AllowAnonymous] // public customs assistant — anyone may ask questions
public class AssistantController : ControllerBase
{
    private readonly IRagClient _ragClient;

    public AssistantController(IRagClient ragClient)
    {
        _ragClient = ragClient;
    }

    // POST /api/assistant/ask
    // Forwards the caller's question to the RAG service and returns the grounded answer.
    // The RAG API key stays server-side — it is never returned to the caller.
    [HttpPost("ask")]
    public async Task<IActionResult> Ask([FromBody] AskRequest request, CancellationToken ct)
    {
        try
        {
            var response = await _ragClient.AskAsync(request.Question, ct);
            return Ok(response);
        }
        catch (RagClientException ex)
        {
            // ex.Message is caller-safe (no key, no internals).
            // SuggestedStatusCode is 503 for rate-limit/timeout, 502 for everything else.
            return StatusCode(ex.SuggestedStatusCode, new { message = ex.Message });
        }
    }
}
