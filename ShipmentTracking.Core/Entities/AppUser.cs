namespace ShipmentTracking.Core.Entities;

public class AppUser
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;     // stored normalized (lowercase, trimmed)
    public string PasswordHash { get; set; } = string.Empty; // always BCrypt / Identity-hashed; never plaintext
    public string Role { get; set; } = string.Empty;         // e.g. "Admin"
}
