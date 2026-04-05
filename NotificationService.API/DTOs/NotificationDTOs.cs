using System.ComponentModel.DataAnnotations;

namespace NotificationService.API.DTOs;

public class SendEmailRequest
{
    [Required]
    [EmailAddress]
    public string To { get; set; } = string.Empty;

    [Required]
    public string Subject { get; set; } = string.Empty;

    [Required]
    public string Body { get; set; } = string.Empty;

    public bool IsHtml { get; set; } = true;
}

public class VerifyOtpRequest
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    [StringLength(6, MinimumLength = 6)]
    public string OtpCode { get; set; } = string.Empty;
}

public class NotificationResponse
{
    public Guid Id { get; set; }
    public string RecipientEmail { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? SentAt { get; set; }
}
