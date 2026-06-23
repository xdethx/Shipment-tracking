using ShipmentTracking.Core.DTOs;

namespace ShipmentTracking.Core.Interfaces;

public interface IShipmentQueries
{
    Task<PublicTrackingResponse?> GetTrackingByNumberAsync(string trackingNumber);
}
