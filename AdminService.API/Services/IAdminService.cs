using AdminService.API.DTOs;

namespace AdminService.API.Services;

public interface IAdminService
{
    Task<IEnumerable<ShipmentRecordResponse>> GetAllShipmentsAsync();
    Task<ShipmentRecordResponse?> ResolveShipmentIssueAsync(int id, ResolveIssueRequest request);
}
