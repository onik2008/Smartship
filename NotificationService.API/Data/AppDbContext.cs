using Microsoft.EntityFrameworkCore;
using NotificationService.API.Entities;

namespace NotificationService.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RecipientEmail).IsRequired().HasMaxLength(256);
            entity.Property(e => e.Subject).IsRequired().HasMaxLength(500);
            entity.Property(e => e.OtpCode).HasMaxLength(10);
            entity.Property(e => e.ErrorMessage).HasMaxLength(2000);
        });
    }
}
