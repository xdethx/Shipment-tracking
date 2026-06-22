using ShipmentTracking.Core.Entities;

namespace ShipmentTracking.Core.Interfaces;

public interface IShipmentRepository
{
    Task AddAsync(Shipment shipment);
    Task<Shipment?> GetByTrackingNumberAsync(string trackingNumber);
    Task<IEnumerable<Shipment>> GetAllAsync();
    void Update(Shipment shipment);
    Task SaveChangesAsync();
}
