using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ShipmentTracking.Core.Auth;
using ShipmentTracking.Core.Configuration;
using ShipmentTracking.Core.Entities;
using ShipmentTracking.Core.Interfaces;
using ShipmentTracking.Core.Services;
using ShipmentTracking.Infrastructure.Data;
using ShipmentTracking.Infrastructure.Queries;
using ShipmentTracking.Infrastructure.Rag;
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

// ── Database provider (SqlServer locally; Sqlite on Render) ───────────────────
var dbProvider     = builder.Configuration["DatabaseProvider"] ?? "SqlServer";
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (string.Equals(dbProvider, "Sqlite", StringComparison.OrdinalIgnoreCase))
        options.UseSqlite(connectionString,
            o => o.MigrationsAssembly("ShipmentTracking.Migrations.Sqlite"));
    else
        options.UseSqlServer(connectionString);
});

// ── JWT settings ──────────────────────────────────────────────────────────────
// Jwt:Key is a SECRET — comes from user-secrets locally / Jwt__Key env var on Render.
// Issuer, Audience, ExpiryHours are non-secret and live in appsettings.json.
var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>() ?? new JwtSettings();
if (string.IsNullOrWhiteSpace(jwtSettings.Key))
    throw new InvalidOperationException(
        "Jwt:Key is not configured. " +
        "Run: dotnet user-secrets set \"Jwt:Key\" \"<32+ char random string>\" " +
        "or set the Jwt__Key environment variable on Render.");

builder.Services.AddSingleton(jwtSettings); // AuthService takes the POCO directly

// ── Authentication + Authorization ───────────────────────────────────────────
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,        // reject expired tokens
            ValidateIssuerSigningKey = true,        // reject tampered tokens
            ValidIssuer              = jwtSettings.Issuer,
            ValidAudience            = jwtSettings.Audience,
            IssuerSigningKey         = new SymmetricSecurityKey(
                                           Encoding.UTF8.GetBytes(jwtSettings.Key)),
            ClockSkew                = TimeSpan.Zero, // no grace period; 8 h means 8 h
        };
    });
builder.Services.AddAuthorization();

// ── RAG settings ─────────────────────────────────────────────────────────────
// Rag:ApiKey is a SECRET — comes from user-secrets locally / Rag__ApiKey env var on Render.
// Rag:BaseUrl is non-secret and lives in appsettings.json.
var ragSettings = builder.Configuration.GetSection("Rag").Get<RagSettings>() ?? new RagSettings();
if (string.IsNullOrWhiteSpace(ragSettings.BaseUrl))
    throw new InvalidOperationException(
        "Rag:BaseUrl is not configured. Add it to appsettings.json.");
if (string.IsNullOrWhiteSpace(ragSettings.ApiKey))
    throw new InvalidOperationException(
        "Rag:ApiKey is not configured. " +
        "Run: dotnet user-secrets set \"Rag:ApiKey\" \"<your key>\" " +
        "or set the Rag__ApiKey environment variable on Render.");

// Typed HttpClient: base address and Authorization header set once here;
// RagClient never touches the key. 100s timeout for cold-starting HF Spaces.
builder.Services.AddHttpClient<IRagClient, RagClient>(client =>
{
    client.BaseAddress = new Uri(ragSettings.BaseUrl.TrimEnd('/') + "/");
    client.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", ragSettings.ApiKey);
    client.Timeout = TimeSpan.FromSeconds(100);
});

// ── DI registrations ─────────────────────────────────────────────────────────
builder.Services.AddSingleton<IPasswordHasher<AppUser>, PasswordHasher<AppUser>>();

builder.Services.AddScoped<IShipmentRepository, ShipmentRepository>();
builder.Services.AddScoped<IShipmentService, ShipmentService>();
builder.Services.AddScoped<IShipmentQueries, ShipmentQueries>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();

var app = builder.Build();

// ── Startup: migrate + seed ───────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db      = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var hasher  = scope.ServiceProvider.GetRequiredService<IPasswordHasher<AppUser>>();
    var config  = scope.ServiceProvider.GetRequiredService<IConfiguration>();

    db.Database.Migrate();
    await AdminUserSeeder.SeedAsync(db, hasher, config);
}

// ── Middleware pipeline ───────────────────────────────────────────────────────
app.UseHttpsRedirection();
app.UseDefaultFiles();   // rewrites "/" -> "/index.html"
app.UseStaticFiles();    // serves everything under wwwroot
app.UseAuthentication(); // must come before UseAuthorization
app.UseAuthorization();
app.MapControllers();

app.Run();
