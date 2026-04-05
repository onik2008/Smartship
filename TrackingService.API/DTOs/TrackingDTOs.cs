using System.ComponentModel.DataAnnotations;

namespace TrackingService.API.DTOs;

public class AddTrackingEventRequest
{
    [Required]
    [MaxLength(50)]
    public string TrackingNumber { get; set; } = string.Empty;

    [Required]
    [MaxLength(300)]
    public string Location { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Status { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;
}

public class TrackingEventResponse
{
    public int Id { get; set; }
    public string TrackingNumber { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime EventTime { get; set; }
}
