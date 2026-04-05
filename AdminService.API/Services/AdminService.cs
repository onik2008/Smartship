using AdminService.API.Data;
using AdminService.API.DTOs;
using AdminService.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace AdminService.API.Services;

public class AdminService : IAdminService
{
    private readonly AppDbContext _context;

    public AdminService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ShipmentRecordResponse>> GetAllShipmentsAsync()
    {
        var records = await _context.ShipmentRecords
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();

        return records.Select(MapToResponse);
    }

    public async Task<ShipmentRecordResponse?> ResolveShipmentIssueAsync(int id, ResolveIssueRequest request)
    {
        var record = await _context.ShipmentRecords.FindAsync(id);
        if (record == null) return null;

        record.IsResolved = true;
        record.IssueDescription = request.Resolution;
        record.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return MapToResponse(record);
    }

    private static ShipmentRecordResponse MapToResponse(ShipmentRecord s) => new()
    {
        Id = s.Id,
        TrackingNumber = s.TrackingNumber,
        SenderName = s.SenderName,
        RecipientName = s.RecipientName,
        Status = s.Status.ToString(),
        IssueDescription = s.IssueDescription,
        IsResolved = s.IsResolved,
        CreatedAt = s.CreatedAt,
        UpdatedAt = s.UpdatedAt
    };
}
