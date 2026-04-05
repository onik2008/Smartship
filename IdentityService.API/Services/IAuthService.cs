using IdentityService.API.DTOs;

namespace IdentityService.API.Services;

public interface IAuthService
{
    Task<RegisterResponse?> RegisterAsync(RegisterRequest request);
    Task<AuthResponse?> VerifyOtpAsync(VerifyOtpRequest request);
    Task<AuthResponse?> LoginAsync(LoginRequest request);
}
