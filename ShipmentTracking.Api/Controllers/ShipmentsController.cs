using Microsoft.AspNetCore.Mvc;
using ShipmentTracking.Api.Filters;
using ShipmentTracking.Core.DTOs;
using ShipmentTracking.Core.Exceptions;
using ShipmentTracking.Core.Interfaces;

namespace ShipmentTracking.Api.Controllers;

[ApiController]
[Route("api/shipments")]
[ApiKeyAuthFilter]
public class ShipmentsController : ControllerBase
{
    private readonly IShipmentService _shipmentService;

    public ShipmentsController(IShipmentService shipmentService)
    {
        _shipmentService = shipmentService;
    }

    // POST /api/shipments
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateShipmentRequest request)
    {
        var response = await _shipmentService.CreateAsync(request);
        return Created($"/api/shipments/{response.TrackingNumber}", response);
    }

    // GET /api/shipments
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var shipments = await _shipmentService.GetAllAsync();
        return Ok(shipments);
    }

    // PUT /api/shipments/{trackingNumber}/status
    [HttpPut("{trackingNumber}/status")]
    public async Task<IActionResult> UpdateStatus(string trackingNumber, [FromBody] UpdateStatusRequest request)
    {
        try
        {
            var response = await _shipmentService.UpdateStatusAsync(trackingNumber, request);
            if (response is null)
                return NotFound();

            return Ok(response);
        }
        catch (InvalidStatusTransitionException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
