namespace TrackingService.API.Entities;

public class TrackingEvent
{
    public int Id { get; set; }
    public string TrackingNumber { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime EventTime { get; set; } = DateTime.UtcNow;
}
