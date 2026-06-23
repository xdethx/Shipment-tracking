using ShipmentTracking.Core.Enums;

namespace ShipmentTracking.Core.DTOs;

public class PublicTrackingResponse
{
    public string TrackingNumber { get; set; } = string.Empty;
    public ShipmentStatus Status { get; set; }
    public string Origin { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<PublicTrackingHistoryItem> History { get; set; } = new();
}
