using SmartShip.Shared.Enums;

namespace NotificationService.API.Entities;

public class Notification
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string RecipientEmail { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public NotificationStatus Status { get; set; } = NotificationStatus.Pending;
    public string? OtpCode { get; set; }
    public DateTime? ExpiryTime { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? SentAt { get; set; }
    public string? ErrorMessage { get; set; }
    public Guid? UserId { get; set; }
}
