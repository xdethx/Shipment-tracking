namespace ShipmentTracking.Core.Configuration;

// Bound from the "Rag" config section in appsettings.json.
// ApiKey is the ONLY secret — it comes from user-secrets locally / Rag__ApiKey env var on Render.
// BaseUrl is non-secret and lives in appsettings.json.
public class RagSettings
{
    public string BaseUrl { get; set; } = string.Empty;
    public string ApiKey  { get; set; } = string.Empty;
}
