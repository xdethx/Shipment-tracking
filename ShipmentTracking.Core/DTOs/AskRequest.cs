using System.ComponentModel.DataAnnotations;

namespace ShipmentTracking.Core.DTOs;

// Inbound DTO — the question a caller sends to POST /api/assistant/ask.
// [Required] ensures ModelState validation rejects an empty body before we call the RAG service.
public class AskRequest
{
    [Required] public string Question { get; set; } = string.Empty;
}
