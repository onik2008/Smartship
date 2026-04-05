using AdminService.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace AdminService.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<ShipmentRecord> ShipmentRecords => Set<ShipmentRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ShipmentRecord>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.TrackingNumber).IsUnique();
            entity.Property(e => e.TrackingNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.SenderName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.RecipientName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.IssueDescription).HasMaxLength(1000);
        });
    }
}
