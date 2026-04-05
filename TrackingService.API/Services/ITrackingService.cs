using TrackingService.API.DTOs;

namespace TrackingService.API.Services;

public interface ITrackingService
{
    Task<TrackingEventResponse> AddTrackingEventAsync(AddTrackingEventRequest request);
    Task<IEnumerable<TrackingEventResponse>> GetTrackingTimelineAsync(string trackingNumber);
}
