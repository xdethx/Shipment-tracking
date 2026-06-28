using System.Text.Json.Serialization;

namespace ShipmentTracking.Core.DTOs;

// One retrieved document chunk the RAG service used to ground its answer.
// Real response shape (confirmed): { "source": "...", "score": 1.03, "text": "..." }
// The JSON key "source" is mapped to Document so it reads clearly at call sites.
public class Source
{
    [JsonPropertyName("source")] public string Document { get; set; } = string.Empty;
    [JsonPropertyName("score")]  public double Score    { get; set; }
    [JsonPropertyName("text")]   public string Text     { get; set; } = string.Empty;
}
