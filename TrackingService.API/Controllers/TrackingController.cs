using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartShip.Shared.DTOs;
using TrackingService.API.DTOs;
using TrackingService.API.Services;

namespace TrackingService.API.Controllers;

[ApiController]
[Route("api/tracking")]
[Authorize]
public class TrackingController : ControllerBase
{
    private readonly ITrackingService _trackingService;

    public TrackingController(ITrackingService trackingService)
    {
        _trackingService = trackingService;
    }

    /// <summary>Add a tracking event</summary>
    [HttpPost("update")]
    [ProducesResponseType(typeof(ApiResponse<TrackingEventResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AddTrackingEvent([FromBody] AddTrackingEventRequest request)
    {
        var result = await _trackingService.AddTrackingEventAsync(request);
        return Ok(ApiResponse<TrackingEventResponse>.SuccessResponse(result, "Tracking event added."));
    }

    /// <summary>Get tracking timeline by tracking number</summary>
    [HttpGet("{trackingNumber}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<TrackingEventResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTrackingTimeline(string trackingNumber)
    {
        var result = await _trackingService.GetTrackingTimelineAsync(trackingNumber);
        var events = result.ToList();

        if (!events.Any())
            return NotFound(ApiResponse<object>.FailureResponse($"No tracking events found for '{trackingNumber}'."));

        return Ok(ApiResponse<IEnumerable<TrackingEventResponse>>.SuccessResponse(events));
    }
}
