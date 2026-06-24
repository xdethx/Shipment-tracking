# ── Build stage ──────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy the three .csproj files into their matching subdirectories so Docker
# can cache the restore layer separately from the source copy.
# Restore is only re-run when a .csproj changes, not on every code edit.
COPY ShipmentTracking.Core/ShipmentTracking.Core.csproj                           ShipmentTracking.Core/
COPY ShipmentTracking.Infrastructure/ShipmentTracking.Infrastructure.csproj       ShipmentTracking.Infrastructure/
COPY ShipmentTracking.Migrations.Sqlite/ShipmentTracking.Migrations.Sqlite.csproj ShipmentTracking.Migrations.Sqlite/
COPY ShipmentTracking.Api/ShipmentTracking.Api.csproj                             ShipmentTracking.Api/

RUN dotnet restore ShipmentTracking.Api/ShipmentTracking.Api.csproj

# Copy the rest of the source and publish in Release mode.
COPY . .
RUN dotnet publish ShipmentTracking.Api/ShipmentTracking.Api.csproj \
    -c Release \
    --no-restore \
    -o /app/publish

# ── Runtime stage ─────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Create a writable directory for the SQLite database file.
# On Render's free tier this filesystem is ephemeral — the file (and all data)
# is lost on every redeploy or after an idle spin-down. Acceptable for a demo.
RUN mkdir -p /app/data

COPY --from=build /app/publish .

# Render injects PORT at runtime and routes public traffic to it.
# The app reads PORT in Program.cs and binds Kestrel to that port.
EXPOSE 8080

ENTRYPOINT ["dotnet", "ShipmentTracking.Api.dll"]
