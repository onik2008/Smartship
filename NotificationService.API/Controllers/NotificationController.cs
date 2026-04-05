using Microsoft.AspNetCore.Mvc;
using NotificationService.API.DTOs;
using NotificationService.API.Services;
using SmartShip.Shared.DTOs;

namespace NotificationService.API.Controllers;

[ApiController]
[Route("api/notifications")]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    /// <summary>Service health check</summary>
    [HttpGet("health")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public IActionResult Health()
    {
        return Ok(ApiResponse<object>.SuccessResponse(new
        {
            service = "NotificationService",
            status = "Healthy",
            timestamp = DateTime.UtcNow
        }));
    }

    /// <summary>Send a custom email (internal use)</summary>
    [HttpPost("send-email")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SendEmail([FromBody] SendEmailRequest request)
    {
        var success = await _notificationService.SendCustomEmailAsync(request);
        if (!success)
            return StatusCode(500, ApiResponse<object>.FailureResponse("Failed to send email."));

        return Ok(ApiResponse<object>.SuccessResponse(new { }, "Email sent successfully."));
    }

    /// <summary>Verify OTP for a user (called by IdentityService)</summary>
    [HttpPost("verify-otp")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
    {
        var isValid = await _notificationService.VerifyOtpAsync(request.UserId, request.OtpCode);
        if (!isValid)
            return BadRequest(ApiResponse<object>.FailureResponse("Invalid or expired OTP."));

        return Ok(ApiResponse<object>.SuccessResponse(new { }, "OTP verified successfully."));
    }
}
