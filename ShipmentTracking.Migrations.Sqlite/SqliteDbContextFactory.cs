using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using ShipmentTracking.Infrastructure.Data;

namespace ShipmentTracking.Migrations.Sqlite;

// Used exclusively by `dotnet ef migrations add` — never called at runtime.
// Tells EF tools to create AppDbContext with the SQLite provider so that
// generated migration SQL uses SQLite-compatible column types (TEXT, INTEGER, NUMERIC)
// rather than SQL Server types (nvarchar(max), datetime2, etc.).
public class SqliteDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(
                "Data Source=design-time.db",
                o => o.MigrationsAssembly("ShipmentTracking.Migrations.Sqlite"))
            .Options;

        return new AppDbContext(options);
    }
}
