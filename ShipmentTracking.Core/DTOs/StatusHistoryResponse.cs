using ShipmentTracking.Core.Enums;

namespace ShipmentTracking.Core.DTOs;

public class StatusHistoryResponse
{
    public ShipmentStatus Status { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }
}
