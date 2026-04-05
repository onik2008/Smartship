using Microsoft.EntityFrameworkCore;
using TrackingService.API.Entities;

namespace TrackingService.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<TrackingEvent> TrackingEvents => Set<TrackingEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TrackingEvent>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.TrackingNumber);
            entity.Property(e => e.TrackingNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Location).IsRequired().HasMaxLength(300);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(1000);
        });
    }
}
