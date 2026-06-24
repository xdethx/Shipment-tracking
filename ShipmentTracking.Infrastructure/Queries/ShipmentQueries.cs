using System.Data.Common;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using ShipmentTracking.Core.DTOs;
using ShipmentTracking.Core.Interfaces;

namespace ShipmentTracking.Infrastructure.Queries;

public class ShipmentQueries : IShipmentQueries
{
    private readonly string _connectionString;
    private readonly string _provider;

    public ShipmentQueries(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection is not configured.");

        _provider = configuration["DatabaseProvider"] ?? "SqlServer";
    }

    // Returns the correct connection type for the configured provider.
    // The SQL itself is identical for both — only the ADO.NET driver differs.
    private DbConnection CreateConnection() =>
        string.Equals(_provider, "Sqlite", StringComparison.OrdinalIgnoreCase)
            ? new SqliteConnection(_connectionString)
            : new SqlConnection(_connectionString);

    public async Task<PublicTrackingResponse?> GetTrackingByNumberAsync(string trackingNumber)
    {
        const string shipmentSql = """
            SELECT Id, TrackingNumber, Status, Origin, Destination, CreatedAt
            FROM   Shipments
            WHERE  TrackingNumber = @TrackingNumber
            """;

        const string historySql = """
            SELECT Status, Note, CreatedAt
            FROM   StatusHistories
            WHERE  ShipmentId = @ShipmentId
            ORDER BY CreatedAt
            """;

        await using var conn = CreateConnection();
        await conn.OpenAsync();

        var row = await conn.QuerySingleOrDefaultAsync<ShipmentRow>(
            shipmentSql, new { TrackingNumber = trackingNumber });

        if (row is null)
            return null;

        var history = await conn.QueryAsync<PublicTrackingHistoryItem>(
            historySql, new { ShipmentId = row.Id });

        return new PublicTrackingResponse
        {
            TrackingNumber = row.TrackingNumber,
            Status         = row.Status,
            Origin         = row.Origin,
            Destination    = row.Destination,
            CreatedAt      = row.CreatedAt,
            History        = history.ToList(),
        };
    }

    // Private read model — holds the internal Id needed for the history query
    // but never exposed to the caller.
    private sealed class ShipmentRow
    {
        public int Id { get; set; }
        public string TrackingNumber { get; set; } = string.Empty;
        public ShipmentTracking.Core.Enums.ShipmentStatus Status { get; set; }
        public string Origin { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
