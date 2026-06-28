using System.Text.Json.Serialization;

namespace ShipmentTracking.Core.DTOs;

// Outbound DTO — mirrors the JSON the RAG service returns on a 200.
// [JsonPropertyName] maps the lowercase JSON keys to idiomatic C# property names.
public class AskResponse
{
    [JsonPropertyName("answer")]  public string       Answer  { get; set; } = string.Empty;
    [JsonPropertyName("sources")] public List<Source> Sources { get; set; } = new();
}
