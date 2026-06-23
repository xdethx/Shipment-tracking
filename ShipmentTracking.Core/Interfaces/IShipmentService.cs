using ShipmentTracking.Core.DTOs;

namespace ShipmentTracking.Core.Interfaces;

public interface IShipmentService
{
    Task<ShipmentResponse> CreateAsync(CreateShipmentRequest request);
    Task<ShipmentResponse?> UpdateStatusAsync(string trackingNumber, UpdateStatusRequest request);
    Task<IEnumerable<ShipmentResponse>> GetAllAsync();
}
