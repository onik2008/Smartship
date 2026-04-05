using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShipmentService.API.DTOs;
using ShipmentService.API.Services;
using SmartShip.Shared.DTOs;

namespace ShipmentService.API.Controllers;

[ApiController]
[Route("api/shipments")]
[Authorize]
public class ShipmentController : ControllerBase
{
    private readonly IShipmentService _shipmentService;

    public ShipmentController(IShipmentService shipmentService)
    {
        _shipmentService = shipmentService;
    }

    /// <summary>Create a new shipment</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ShipmentResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateShipment([FromBody] CreateShipmentRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var result = await _shipmentService.CreateShipmentAsync(request, userId);
        return CreatedAtAction(nameof(GetShipment), new { id = result.Id },
            ApiResponse<ShipmentResponse>.SuccessResponse(result, "Shipment created successfully."));
    }

    /// <summary>Get shipment details by ID</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<ShipmentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetShipment(int id)
    {
        var result = await _shipmentService.GetShipmentByIdAsync(id);
        if (result == null)
            return NotFound(ApiResponse<object>.FailureResponse($"Shipment {id} not found."));

        return Ok(ApiResponse<ShipmentResponse>.SuccessResponse(result));
    }
}
