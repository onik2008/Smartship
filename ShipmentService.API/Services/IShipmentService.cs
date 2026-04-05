using ShipmentService.API.DTOs;

namespace ShipmentService.API.Services;

public interface IShipmentService
{
    Task<ShipmentResponse> CreateShipmentAsync(CreateShipmentRequest request, int userId);
    Task<ShipmentResponse?> GetShipmentByIdAsync(int id);
}
