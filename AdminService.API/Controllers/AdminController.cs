using AdminService.API.DTOs;
using AdminService.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartShip.Shared.Constants;
using SmartShip.Shared.DTOs;

namespace AdminService.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = AppConstants.AdminRole)]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    /// <summary>Get all shipments (Admin only)</summary>
    [HttpGet("shipments")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ShipmentRecordResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAllShipments()
    {
        var result = await _adminService.GetAllShipmentsAsync();
        return Ok(ApiResponse<IEnumerable<ShipmentRecordResponse>>.SuccessResponse(result));
    }

    /// <summary>Resolve a shipment issue (Admin only)</summary>
    [HttpPut("shipments/{id:int}/resolve")]
    [ProducesResponseType(typeof(ApiResponse<ShipmentRecordResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ResolveShipmentIssue(int id, [FromBody] ResolveIssueRequest request)
    {
        var result = await _adminService.ResolveShipmentIssueAsync(id, request);
        if (result == null)
            return NotFound(ApiResponse<object>.FailureResponse($"Shipment record {id} not found."));

        return Ok(ApiResponse<ShipmentRecordResponse>.SuccessResponse(result, "Issue resolved successfully."));
    }
}
