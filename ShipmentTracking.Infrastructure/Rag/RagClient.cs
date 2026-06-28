using System.Net;
using System.Net.Http.Json;
using ShipmentTracking.Core.DTOs;
using ShipmentTracking.Core.Interfaces;
using ShipmentTracking.Infrastructure.Exceptions;

namespace ShipmentTracking.Infrastructure.Rag;

// Implements IRagClient using a typed HttpClient.
// The HttpClient is configured in Program.cs (base address + Bearer header + timeout)
// so this class knows nothing about the RAG URL or the API key — it only handles
// the HTTP mechanics and maps error outcomes to RagClientException.
public class RagClient : IRagClient
{
    private readonly HttpClient _http;

    public RagClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<AskResponse> AskAsync(string question, CancellationToken ct)
    {
        HttpResponseMessage response;

        try
        {
            // PostAsJsonAsync serialises { "question": "..." } and sets Content-Type: application/json.
            // The path "ask" is relative; it resolves against the BaseAddress set in Program.cs.
            response = await _http.PostAsJsonAsync("ask", new AskRequest { Question = question }, ct);
        }
        catch (TaskCanceledException) when (!ct.IsCancellationRequested)
        {
            // The HttpClient's own timeout fired (not the caller cancelling).
            throw new RagClientException(
                "The assistant did not respond in time. Please try again in a moment.", 503);
        }

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<AskResponse>(ct);

            // Guard against a 200 with an empty/null body (shouldn't happen, but be safe).
            return result
                ?? throw new RagClientException("The assistant returned an empty response.", 502);
        }

        // Map known error codes to a clean, caller-safe message.
        throw response.StatusCode switch
        {
            HttpStatusCode.Unauthorized => new RagClientException(
                "The assistant is not configured correctly. Contact the administrator.", 502),

            HttpStatusCode.TooManyRequests => new RagClientException(
                "The assistant is busy right now. Please try again in a moment.", 503),

            _ => new RagClientException(
                "The assistant is temporarily unavailable. Please try again later.", 502),
        };
    }
}
