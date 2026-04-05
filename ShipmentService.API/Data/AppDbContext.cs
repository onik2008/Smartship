using Microsoft.EntityFrameworkCore;
using ShipmentService.API.Entities;

namespace ShipmentService.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Shipment> Shipments => Set<Shipment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Shipment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.TrackingNumber).IsUnique();
            entity.Property(e => e.TrackingNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.SenderName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.SenderAddress).IsRequired().HasMaxLength(500);
            entity.Property(e => e.RecipientName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.RecipientAddress).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Description).HasMaxLength(1000);
        });
    }
}
