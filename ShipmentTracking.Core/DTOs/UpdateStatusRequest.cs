using ShipmentTracking.Core.Enums;

namespace ShipmentTracking.Core.DTOs;

public class UpdateStatusRequest
{
    public ShipmentStatus NewStatus { get; set; }
    public string? Note { get; set; }
}
