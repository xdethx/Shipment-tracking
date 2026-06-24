using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ShipmentTracking.Infrastructure.Data;

// Used ONLY by `dotnet ef migrations add` targeting ShipmentTracking.Infrastructure.
// Never called at runtime. Provides a SQL Server AppDbContext without needing
// Program.cs to boot, which avoids the Jwt:Key startup validation blocking
// the EF design-time tooling.
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(
                "Server=(localdb)\\mssqllocaldb;Database=ShipmentTrackingDb;" +
                "Trusted_Connection=True;TrustServerCertificate=True")
            .Options;

        return new AppDbContext(options);
    }
}
