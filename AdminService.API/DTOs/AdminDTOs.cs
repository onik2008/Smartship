using System.ComponentModel.DataAnnotations;

namespace AdminService.API.DTOs;

public class ShipmentRecordResponse
{
    public int Id { get; set; }
    public string TrackingNumber { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string RecipientName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? IssueDescription { get; set; }
    public bool IsResolved { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class ResolveIssueRequest
{
    [Required]
    [MaxLength(1000)]
    public string Resolution { get; set; } = string.Empty;
}
