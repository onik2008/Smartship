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

    /// <summary>Register a new user</summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);
        if (result == null)
            return BadRequest(ApiResponse<object>.FailureResponse("Email already registered."));

        return Ok(ApiResponse<AuthResponse>.SuccessResponse(result, "User registered successfully."));
    }

    /// <summary>Login and get JWT token</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        if (result == null)
            return Unauthorized(ApiResponse<object>.FailureResponse("Invalid email or password."));

        return Ok(ApiResponse<AuthResponse>.SuccessResponse(result, "Login successful."));
    }
}
