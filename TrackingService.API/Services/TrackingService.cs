using Microsoft.EntityFrameworkCore;
using TrackingService.API.Data;
using TrackingService.API.DTOs;
using TrackingService.API.Entities;

namespace TrackingService.API.Services;

public class TrackingService : ITrackingService
{
    private readonly AppDbContext _context;

    public TrackingService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<TrackingEventResponse> AddTrackingEventAsync(AddTrackingEventRequest request)
    {
        var trackingEvent = new TrackingEvent
        {
            TrackingNumber = request.TrackingNumber,
            Location = request.Location,
            Status = request.Status,
            Description = request.Description,
            EventTime = DateTime.UtcNow
        };

        _context.TrackingEvents.Add(trackingEvent);
        await _context.SaveChangesAsync();

        return MapToResponse(trackingEvent);
    }

    public async Task<IEnumerable<TrackingEventResponse>> GetTrackingTimelineAsync(string trackingNumber)
    {
        var events = await _context.TrackingEvents
            .Where(e => e.TrackingNumber == trackingNumber)
            .OrderBy(e => e.EventTime)
            .ToListAsync();

        return events.Select(MapToResponse);
    }

    private static TrackingEventResponse MapToResponse(TrackingEvent e) => new()
    {
        Id = e.Id,
        TrackingNumber = e.TrackingNumber,
        Location = e.Location,
        Status = e.Status,
        Description = e.Description,
        EventTime = e.EventTime
    };
}
