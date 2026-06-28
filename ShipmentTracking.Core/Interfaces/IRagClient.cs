using ShipmentTracking.Core.DTOs;

namespace ShipmentTracking.Core.Interfaces;

// Contract between the Api layer and the RAG service.
// The controller depends only on this interface — it never knows whether
// the implementation uses HttpClient, a mock, or anything else.
public interface IRagClient
{
    // Sends question to the RAG service and returns the grounded answer + sources.
    // Throws RagClientException on any non-success outcome (401, 429, timeout, 5xx).
    Task<AskResponse> AskAsync(string question, CancellationToken ct);
}
