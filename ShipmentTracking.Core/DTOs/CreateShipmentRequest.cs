using System.ComponentModel.DataAnnotations;

namespace ShipmentTracking.Core.DTOs;

public class CreateShipmentRequest
{
    [Required]
    public string SenderName { get; set; } = string.Empty;

    [Required]
    public string ReceiverName { get; set; } = string.Empty;

    [Required]
    public string Origin { get; set; } = string.Empty;

    [Required]
    public string Destination { get; set; } = string.Empty;
}
