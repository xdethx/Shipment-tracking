using ShipmentTracking.Core.DTOs;

namespace ShipmentTracking.Core.Interfaces;

public interface IAuthService
{
    // Returns null on any auth failure (wrong user OR wrong password).
    // The controller maps null -> 401 with a generic message so callers cannot
    // distinguish "user not found" from "wrong password".
    Task<LoginResponse?> LoginAsync(LoginRequest request);
}
