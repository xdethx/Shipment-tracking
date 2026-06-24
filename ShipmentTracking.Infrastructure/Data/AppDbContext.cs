using Microsoft.EntityFrameworkCore;
using ShipmentTracking.Core.Entities;

namespace ShipmentTracking.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Shipment> Shipments => Set<Shipment>();
    public DbSet<ShipmentStatusHistory> StatusHistories => Set<ShipmentStatusHistory>();
    public DbSet<AppUser> Users => Set<AppUser>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Shipment>(entity =>
        {
            entity.Property(s => s.TrackingNumber)
                  .HasMaxLength(50)
                  .IsRequired();

            entity.HasIndex(s => s.TrackingNumber)
                  .IsUnique();

            entity.HasMany(s => s.StatusHistory)
                  .WithOne(h => h.Shipment)
                  .HasForeignKey(h => h.ShipmentId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.Property(u => u.Username)
                  .HasMaxLength(256)
                  .IsRequired();

            entity.Property(u => u.PasswordHash)
                  .IsRequired();

            entity.Property(u => u.Role)
                  .HasMaxLength(50)
                  .IsRequired();

            entity.HasIndex(u => u.Username)
                  .IsUnique();
        });
    }
}
