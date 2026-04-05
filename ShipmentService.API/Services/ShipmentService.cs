using ShipmentService.API.Data;
using ShipmentService.API.DTOs;
using ShipmentService.API.Entities;
using Microsoft.EntityFrameworkCore;
using SmartShip.Shared.Enums;

namespace ShipmentService.API.Services;

public class ShipmentService : IShipmentService
{
    private readonly AppDbContext _context;

    public ShipmentService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ShipmentResponse> CreateShipmentAsync(CreateShipmentRequest request, int userId)
    {
        var shipment = new Shipment
        {
            TrackingNumber = GenerateTrackingNumber(),
            SenderName = request.SenderName,
            SenderAddress = request.SenderAddress,
            RecipientName = request.RecipientName,
            RecipientAddress = request.RecipientAddress,
            WeightKg = request.WeightKg,
            Description = request.Description,
            Status = ShipmentStatus.Booked,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedByUserId = userId
        };

        _context.Shipments.Add(shipment);
        await _context.SaveChangesAsync();

        return MapToResponse(shipment);
    }

    public async Task<ShipmentResponse?> GetShipmentByIdAsync(int id)
    {
        var shipment = await _context.Shipments.FindAsync(id);
        return shipment == null ? null : MapToResponse(shipment);
    }

    private static string GenerateTrackingNumber()
        => $"SS{DateTime.UtcNow:yyyyMMddHHmmss}{Random.Shared.Next(1000, 9999)}";

    private static ShipmentResponse MapToResponse(Shipment s) => new()
    {
        Id = s.Id,
        TrackingNumber = s.TrackingNumber,
        SenderName = s.SenderName,
        SenderAddress = s.SenderAddress,
        RecipientName = s.RecipientName,
        RecipientAddress = s.RecipientAddress,
        WeightKg = s.WeightKg,
        Description = s.Description,
        Status = s.Status.ToString(),
        CreatedAt = s.CreatedAt,
        UpdatedAt = s.UpdatedAt
    };
}
