using Microsoft.EntityFrameworkCore;
using ShipmentTracking.Core.Interfaces;
using ShipmentTracking.Core.Services;
using ShipmentTracking.Infrastructure.Data;
using ShipmentTracking.Infrastructure.Queries;
using ShipmentTracking.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(
        new System.Text.Json.Serialization.JsonStringEnumConverter()));

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IShipmentRepository, ShipmentRepository>();
builder.Services.AddScoped<IShipmentService, ShipmentService>();
builder.Services.AddScoped<IShipmentQueries, ShipmentQueries>();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseDefaultFiles();   // rewrites "/" -> "/index.html"
app.UseStaticFiles();    // serves everything under wwwroot
app.UseAuthorization();
app.MapControllers();

app.Run();
