using ShipmentTracking.Core.Enums;

namespace ShipmentTracking.Core.DTOs;

public class ShipmentResponse
{
    public int Id { get; set; }
    public string TrackingNumber { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string ReceiverName { get; set; } = string.Empty;
    public string Origin { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public ShipmentStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<StatusHistoryResponse> History { get; set; } = new();
}
