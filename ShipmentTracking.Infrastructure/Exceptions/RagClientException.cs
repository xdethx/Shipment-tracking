namespace ShipmentTracking.Infrastructure.Exceptions;

// Thrown by RagClient when the RAG service returns a non-success response,
// times out, or cannot be reached.
// The Message is caller-safe — it never contains the API key or internal details.
// SuggestedStatusCode lets the controller pick an appropriate HTTP status to return
// to the caller (503 for rate-limit, 502 for everything else).
public class RagClientException : Exception
{
    public int SuggestedStatusCode { get; }

    public RagClientException(string message, int suggestedStatusCode = 502)
        : base(message)
    {
        SuggestedStatusCode = suggestedStatusCode;
    }
}
