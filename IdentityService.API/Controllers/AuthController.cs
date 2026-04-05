using IdentityService.API.DTOs;
using IdentityService.API.Services;
using Microsoft.AspNetCore.Mvc;
using SmartShip.Shared.DTOs;

namespace IdentityService.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>Register a new user (publishes OTP to queue)</summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<RegisterResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);
        if (result == null)
            return BadRequest(ApiResponse<object>.FailureResponse("Email already registered."));

        return Ok(ApiResponse<RegisterResponse>.SuccessResponse(result, result.Message));
    }

    /// <summary>Verify OTP and complete registration</summary>
    [HttpPost("verify-otp")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
    {
        var result = await _authService.VerifyOtpAsync(request);
        if (result == null)
            return BadRequest(ApiResponse<object>.FailureResponse("Invalid or expired OTP."));

        return Ok(ApiResponse<AuthResponse>.SuccessResponse(result, "Email verified successfully. You are now logged in."));
    }

    /// <summary>Login and get JWT token</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        if (result == null)
            return Unauthorized(ApiResponse<object>.FailureResponse("Invalid credentials or account not verified."));

        return Ok(ApiResponse<AuthResponse>.SuccessResponse(result, "Login successful."));
    }
}
