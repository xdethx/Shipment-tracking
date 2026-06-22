using Microsoft.EntityFrameworkCore;
using ShipmentTracking.Core.Entities;

namespace ShipmentTracking.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Shipment> Shipments => Set<Shipment>();
    public DbSet<ShipmentStatusHistory> StatusHistories => Set<ShipmentStatusHistory>();

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
    }
}
