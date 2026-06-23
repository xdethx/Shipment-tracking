using ShipmentTracking.Core.DTOs;
using ShipmentTracking.Core.Entities;
using ShipmentTracking.Core.Enums;
using ShipmentTracking.Core.Exceptions;
using ShipmentTracking.Core.Interfaces;

namespace ShipmentTracking.Core.Services;

public class ShipmentService : IShipmentService
{
    private const string TrackingNumberCharset = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    private const int TrackingNumberSuffixLength = 6;

    // Forward-only allowed transitions — one readable place for the business rule.
    private static readonly Dictionary<ShipmentStatus, ShipmentStatus> AllowedTransitions = new()
    {
        { ShipmentStatus.Created,        ShipmentStatus.AtCustoms      },
        { ShipmentStatus.AtCustoms,      ShipmentStatus.InTransit      },
        { ShipmentStatus.InTransit,      ShipmentStatus.OutForDelivery },
        { ShipmentStatus.OutForDelivery, ShipmentStatus.Delivered      },
    };

    private readonly IShipmentRepository _repository;

    public ShipmentService(IShipmentRepository repository)
    {
        _repository = repository;
    }

    public async Task<ShipmentResponse> CreateAsync(CreateShipmentRequest request)
    {
        var trackingNumber = await GenerateUniqueTrackingNumberAsync();
        var now = DateTime.UtcNow;

        var shipment = new Shipment
        {
            TrackingNumber = trackingNumber,
            SenderName     = request.SenderName,
            ReceiverName   = request.ReceiverName,
            Origin         = request.Origin,
            Destination    = request.Destination,
            Status         = ShipmentStatus.Created,
            CreatedAt      = now,
            StatusHistory  =
            [
                new ShipmentStatusHistory
                {
                    Status    = ShipmentStatus.Created,
                    Note      = "Shipment created",
                    CreatedAt = now,
                }
            ]
        };

        await _repository.AddAsync(shipment);
        await _repository.SaveChangesAsync();

        return MapToResponse(shipment);
    }

    public async Task<ShipmentResponse?> UpdateStatusAsync(string trackingNumber, UpdateStatusRequest request)
    {
        var shipment = await _repository.GetByTrackingNumberAsync(trackingNumber);
        if (shipment is null)
            return null;

        ValidateTransition(shipment.Status, request.NewStatus);

        shipment.Status = request.NewStatus;
        shipment.StatusHistory.Add(new ShipmentStatusHistory
        {
            Status    = request.NewStatus,
            Note      = request.Note,
            CreatedAt = DateTime.UtcNow,
        });

        _repository.Update(shipment);
        await _repository.SaveChangesAsync();

        return MapToResponse(shipment);
    }

    public async Task<IEnumerable<ShipmentResponse>> GetAllAsync()
    {
        var shipments = await _repository.GetAllAsync();
        return shipments.Select(MapToResponse);
    }

    // --- Private helpers ---

    private async Task<string> GenerateUniqueTrackingNumberAsync()
    {
        const int maxAttempts = 5;
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            var candidate = BuildTrackingNumber();
            if (await _repository.GetByTrackingNumberAsync(candidate) is null)
                return candidate;
        }
        // The DB unique index is the final backstop; this path is astronomically rare.
        throw new InvalidOperationException("Failed to generate a unique tracking number after multiple attempts.");
    }

    private static string BuildTrackingNumber()
    {
        var suffix = new char[TrackingNumberSuffixLength];
        for (int i = 0; i < suffix.Length; i++)
            suffix[i] = TrackingNumberCharset[Random.Shared.Next(TrackingNumberCharset.Length)];

        return $"TR{DateTime.UtcNow:yyyyMMdd}-{new string(suffix)}";
    }

    private static void ValidateTransition(ShipmentStatus current, ShipmentStatus requested)
    {
        if (!AllowedTransitions.TryGetValue(current, out var allowedNext) || allowedNext != requested)
            throw new InvalidStatusTransitionException(current, requested);
    }

    private static ShipmentResponse MapToResponse(Shipment shipment) => new()
    {
        Id             = shipment.Id,
        TrackingNumber = shipment.TrackingNumber,
        SenderName     = shipment.SenderName,
        ReceiverName   = shipment.ReceiverName,
        Origin         = shipment.Origin,
        Destination    = shipment.Destination,
        Status         = shipment.Status,
        CreatedAt      = shipment.CreatedAt,
        History        = shipment.StatusHistory
                             .OrderBy(h => h.CreatedAt)
                             .Select(MapToHistory)
                             .ToList(),
    };

    private static StatusHistoryResponse MapToHistory(ShipmentStatusHistory h) => new()
    {
        Status    = h.Status,
        Note      = h.Note,
        CreatedAt = h.CreatedAt,
    };
}
