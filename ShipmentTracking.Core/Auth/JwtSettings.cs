namespace ShipmentTracking.Core.Auth;

// Bound from the "Jwt" config section in appsettings.json.
// Key is the ONLY secret — it comes from user-secrets locally / Jwt__Key env var on Render.
// Issuer, Audience, and ExpiryHours are non-secret and live in appsettings.json.
public class JwtSettings
{
    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpiryHours { get; set; } = 8;
}
