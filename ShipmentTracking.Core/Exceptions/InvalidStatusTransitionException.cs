using ShipmentTracking.Core.Enums;

namespace ShipmentTracking.Core.Exceptions;

public class InvalidStatusTransitionException : Exception
{
    public InvalidStatusTransitionException(ShipmentStatus from, ShipmentStatus to)
        : base($"Cannot transition from '{from}' to '{to}'. " +
               $"Allowed transitions: Created→AtCustoms, AtCustoms→InTransit, " +
               $"InTransit→OutForDelivery, OutForDelivery→Delivered.")
    {
    }
}
