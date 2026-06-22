using Microsoft.EntityFrameworkCore;
using ShipmentTracking.Core.Entities;
using ShipmentTracking.Core.Interfaces;
using ShipmentTracking.Infrastructure.Data;

namespace ShipmentTracking.Infrastructure.Repositories;

public class ShipmentRepository : IShipmentRepository
{
    private readonly AppDbContext _context;

    public ShipmentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Shipment shipment)
    {
        await _context.Shipments.AddAsync(shipment);
    }

    public async Task<Shipment?> GetByTrackingNumberAsync(string trackingNumber)
    {
        return await _context.Shipments
            .Include(s => s.StatusHistory)
            .FirstOrDefaultAsync(s => s.TrackingNumber == trackingNumber);
    }

    public async Task<IEnumerable<Shipment>> GetAllAsync()
    {
        return await _context.Shipments
            .Include(s => s.StatusHistory)
            .ToListAsync();
    }

    public void Update(Shipment shipment)
    {
        _context.Shipments.Update(shipment);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
