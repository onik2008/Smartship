using SmartShip.Shared.Enums;

namespace ShipmentService.API.Entities;

public class Shipment
{
    public int Id { get; set; }
    public string TrackingNumber { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string SenderAddress { get; set; } = string.Empty;
    public string RecipientName { get; set; } = string.Empty;
    public string RecipientAddress { get; set; } = string.Empty;
    public double WeightKg { get; set; }
    public string Description { get; set; } = string.Empty;
    public ShipmentStatus Status { get; set; } = ShipmentStatus.Draft;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public int CreatedByUserId { get; set; }
}
