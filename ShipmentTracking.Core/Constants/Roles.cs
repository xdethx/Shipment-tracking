namespace ShipmentTracking.Core.Constants;

// Single source of truth for role name strings.
// Referenced by AuthService (when issuing the role claim) and
// ShipmentsController (in [Authorize(Roles = ...)]).
public static class Roles
{
    public const string Admin = "Admin";
}
