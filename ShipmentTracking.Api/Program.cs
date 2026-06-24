using Microsoft.EntityFrameworkCore;
using ShipmentTracking.Core.Interfaces;
using ShipmentTracking.Core.Services;
using ShipmentTracking.Infrastructure.Data;
using ShipmentTracking.Infrastructure.Queries;
using ShipmentTracking.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Render injects PORT; bind to it so traffic reaches the container.
// Falls back to launchSettings when PORT is absent (local dev).
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
    builder.WebHost.UseUrls($"http://+:{port}");

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(
        new System.Text.Json.Serialization.JsonStringEnumConverter()));

// Choose the database provider from configuration.
// Default (unset) = SqlServer (local dev with LocalDB, unchanged).
// Deployed Render environment sets DatabaseProvider=Sqlite via dashboard env vars.
var dbProvider = builder.Configuration["DatabaseProvider"] ?? "SqlServer";
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (string.Equals(dbProvider, "Sqlite", StringComparison.OrdinalIgnoreCase))
        options.UseSqlite(connectionString,
            o => o.MigrationsAssembly("ShipmentTracking.Migrations.Sqlite"));
    else
        options.UseSqlServer(connectionString);
});

builder.Services.AddScoped<IShipmentRepository, ShipmentRepository>();
builder.Services.AddScoped<IShipmentService, ShipmentService>();
builder.Services.AddScoped<IShipmentQueries, ShipmentQueries>();

var app = builder.Build();

// Apply pending EF Core migrations on startup.
// On the first container run this creates the SQLite file and the full schema.
// On subsequent runs EF checks the __EFMigrationsHistory table and is a no-op.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.UseHttpsRedirection();
app.UseDefaultFiles();   // rewrites "/" -> "/index.html"
app.UseStaticFiles();    // serves everything under wwwroot
app.UseAuthorization();
app.MapControllers();

app.Run();
