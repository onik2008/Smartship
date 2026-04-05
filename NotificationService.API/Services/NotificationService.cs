using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using NotificationService.API.Data;
using NotificationService.API.DTOs;
using NotificationService.API.Entities;
using SmartShip.Shared.Enums;
using SmartShip.Shared.Models;

namespace NotificationService.API.Services;

public interface INotificationService
{
    Task ProcessOtpRequestAsync(OtpRequestMessage message);
    Task ProcessEmailRequestAsync(EmailRequestMessage message);
    Task<bool> VerifyOtpAsync(Guid userId, string otpCode);
    Task<bool> SendCustomEmailAsync(SendEmailRequest request);
}

public class NotificationService : INotificationService
{
    private readonly AppDbContext _context;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        AppDbContext context,
        IEmailService emailService,
        IConfiguration configuration,
        ILogger<NotificationService> logger)
    {
        _context = context;
        _emailService = emailService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task ProcessOtpRequestAsync(OtpRequestMessage message)
    {
        var otpConfig = _configuration.GetSection("Otp");
        int otpLength = int.TryParse(otpConfig["Length"], out var len) ? len : 6;
        int expiryMinutes = int.TryParse(otpConfig["ExpiryMinutes"], out var exp) ? exp : 15;

        var otpCode = GenerateOtp(otpLength);

        var notification = new Notification
        {
            RecipientEmail = message.Email,
            Subject = "SmartShip - Your OTP Verification Code",
            Type = NotificationType.OTP,
            Status = NotificationStatus.Pending,
            OtpCode = otpCode,
            ExpiryTime = DateTime.UtcNow.AddMinutes(expiryMinutes),
            UserId = message.UserId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        var emailBody = BuildOtpEmailBody(message.FirstName, otpCode, expiryMinutes);

        try
        {
            await _emailService.SendEmailAsync(
                message.Email,
                "SmartShip - Your OTP Verification Code",
                emailBody,
                isHtml: true);

            notification.Status = NotificationStatus.Sent;
            notification.SentAt = DateTime.UtcNow;
            _logger.LogInformation("OTP email sent to recipient.");
        }
        catch (Exception ex)
        {
            notification.Status = NotificationStatus.Failed;
            notification.ErrorMessage = ex.Message;
            _logger.LogError(ex, "Failed to send OTP email.");
        }

        await _context.SaveChangesAsync();
    }

    public async Task ProcessEmailRequestAsync(EmailRequestMessage message)
    {
        var notification = new Notification
        {
            RecipientEmail = message.To,
            Subject = message.Subject,
            Type = NotificationType.EMAIL,
            Status = NotificationStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        try
        {
            await _emailService.SendEmailAsync(message.To, message.Subject, message.Body, message.IsHtml);
            notification.Status = NotificationStatus.Sent;
            notification.SentAt = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            notification.Status = NotificationStatus.Failed;
            notification.ErrorMessage = ex.Message;
            _logger.LogError(ex, "Failed to send email.");
        }

        await _context.SaveChangesAsync();
    }

    public async Task<bool> VerifyOtpAsync(Guid userId, string otpCode)
    {
        var notification = await _context.Notifications
            .Where(n => n.UserId == userId
                        && n.Type == NotificationType.OTP
                        && n.OtpCode == otpCode
                        && n.Status == NotificationStatus.Sent
                        && n.ExpiryTime > DateTime.UtcNow)
            .OrderByDescending(n => n.CreatedAt)
            .FirstOrDefaultAsync();

        if (notification == null)
            return false;

        notification.OtpCode = null;
        notification.ExpiryTime = null;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> SendCustomEmailAsync(SendEmailRequest request)
    {
        var notification = new Notification
        {
            RecipientEmail = request.To,
            Subject = request.Subject,
            Type = NotificationType.EMAIL,
            Status = NotificationStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        try
        {
            await _emailService.SendEmailAsync(request.To, request.Subject, request.Body, request.IsHtml);
            notification.Status = NotificationStatus.Sent;
            notification.SentAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            notification.Status = NotificationStatus.Failed;
            notification.ErrorMessage = ex.Message;
            await _context.SaveChangesAsync();
            _logger.LogError(ex, "Failed to send custom email.");
            return false;
        }
    }

    private static string GenerateOtp(int length)
    {
        var digits = new char[length];
        for (int i = 0; i < length; i++)
            digits[i] = (char)('0' + RandomNumberGenerator.GetInt32(0, 10));
        return new string(digits);
    }

    private static string BuildOtpEmailBody(string firstName, string otpCode, int expiryMinutes) =>
        $"""
        <html>
        <body style="font-family: Arial, sans-serif; color: #333;">
            <h2>Hello, {firstName}!</h2>
            <p>Thank you for registering with <strong>SmartShip Logistics</strong>.</p>
            <p>Your One-Time Password (OTP) for email verification is:</p>
            <h1 style="color: #2196F3; letter-spacing: 8px;">{otpCode}</h1>
            <p>This OTP is valid for <strong>{expiryMinutes} minutes</strong>.</p>
            <p>If you did not request this, please ignore this email.</p>
            <hr/>
            <p style="font-size: 12px; color: #888;">SmartShip Logistics Management System</p>
        </body>
        </html>
        """;
}
