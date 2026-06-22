using ShipmentTracking.Core.Enums;

namespace ShipmentTracking.Core.Entities;

public class ShipmentStatusHistory
{
    public int Id { get; set; }
    public int ShipmentId { get; set; }
    public ShipmentStatus Status { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }

    public Shipment Shipment { get; set; } = null!;
}
