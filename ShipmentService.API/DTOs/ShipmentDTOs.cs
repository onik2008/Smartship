using System.ComponentModel.DataAnnotations;

namespace ShipmentService.API.DTOs;

public class CreateShipmentRequest
{
    [Required]
    [MaxLength(200)]
    public string SenderName { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string SenderAddress { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string RecipientName { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string RecipientAddress { get; set; } = string.Empty;

    [Range(0.01, 10000)]
    public double WeightKg { get; set; }

    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;
}

public class ShipmentResponse
{
    public int Id { get; set; }
    public string TrackingNumber { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string SenderAddress { get; set; } = string.Empty;
    public string RecipientName { get; set; } = string.Empty;
    public string RecipientAddress { get; set; } = string.Empty;
    public double WeightKg { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
