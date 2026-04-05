using SmartShip.Shared.Enums;

namespace AdminService.API.Entities;

public class ShipmentRecord
{
    public int Id { get; set; }
    public string TrackingNumber { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string RecipientName { get; set; } = string.Empty;
    public ShipmentStatus Status { get; set; } = ShipmentStatus.Draft;
    public string? IssueDescription { get; set; }
    public bool IsResolved { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
