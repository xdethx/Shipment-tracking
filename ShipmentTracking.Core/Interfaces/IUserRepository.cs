using ShipmentTracking.Core.Entities;

namespace ShipmentTracking.Core.Interfaces;

public interface IUserRepository
{
    Task<AppUser?> GetByUsernameAsync(string normalizedUsername);
}
