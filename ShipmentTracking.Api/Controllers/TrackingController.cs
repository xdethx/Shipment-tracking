using Microsoft.AspNetCore.Mvc;
using ShipmentTracking.Core.Interfaces;

namespace ShipmentTracking.Api.Controllers;

[ApiController]
[Route("api/track")]
public class TrackingController : ControllerBase
{
    private readonly IShipmentQueries _shipmentQueries;

    public TrackingController(IShipmentQueries shipmentQueries)
    {
        _shipmentQueries = shipmentQueries;
    }

    // GET /api/track/{trackingNumber}  — public, no API-key required
    [HttpGet("{trackingNumber}")]
    public async Task<IActionResult> Track(string trackingNumber)
    {
        var response = await _shipmentQueries.GetTrackingByNumberAsync(trackingNumber);
        if (response is null)
            return NotFound();

        return Ok(response);
    }
}
