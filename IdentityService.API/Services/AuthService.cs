using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using IdentityService.API.Data;
using IdentityService.API.DTOs;
using IdentityService.API.Entities;
using IdentityService.API.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SmartShip.Shared.Models;

namespace IdentityService.API.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly IRabbitMqPublisher _publisher;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        AppDbContext context,
        IConfiguration configuration,
        IRabbitMqPublisher publisher,
        ILogger<AuthService> logger)
    {
        _context = context;
        _configuration = configuration;
        _publisher = publisher;
        _logger = logger;
    }

    public async Task<RegisterResponse?> RegisterAsync(RegisterRequest request)
    {
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            return null;

        var user = new User
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = request.Role == "Admin" ? "Admin" : "Customer",
            IsVerified = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var otpMessage = new OtpRequestMessage
        {
            UserId = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            RequestId = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow
        };

        await _publisher.PublishOtpRequestAsync(otpMessage);
        _logger.LogInformation("Registration initiated for {Email}. OTP request published.", user.Email);

        return new RegisterResponse
        {
            Email = user.Email,
            Message = "Registration initiated. Please check your email for OTP verification."
        };
    }

    public async Task<AuthResponse?> VerifyOtpAsync(VerifyOtpRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null)
            return null;

        if (user.IsVerified)
            return GenerateToken(user);

        // Delegate OTP validation to NotificationService via HTTP
        var notificationBaseUrl = _configuration["NotificationService:BaseUrl"] ?? "http://localhost:5005";
        using var httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri(notificationBaseUrl);

        var verifyPayload = new { userId = user.Id, otpCode = request.OtpCode };
        var response = await httpClient.PostAsJsonAsync("/api/notifications/verify-otp", verifyPayload);

        if (!response.IsSuccessStatusCode)
            return null;

        user.IsVerified = true;
        await _context.SaveChangesAsync();

        _logger.LogInformation("User {Email} verified successfully.", user.Email);
        return GenerateToken(user);
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return null;

        if (!user.IsVerified)
            return null;

        return GenerateToken(user);
    }

    private AuthResponse GenerateToken(User user)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddHours(double.Parse(jwtSettings["ExpiryHours"] ?? "24"));

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: creds);

        return new AuthResponse
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            Email = user.Email,
            Role = user.Role,
            ExpiresAt = expires
        };
    }
}
