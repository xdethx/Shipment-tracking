using Microsoft.EntityFrameworkCore;
using ShipmentTracking.Core.Entities;
using ShipmentTracking.Core.Interfaces;
using ShipmentTracking.Infrastructure.Data;

namespace ShipmentTracking.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AppUser?> GetByUsernameAsync(string normalizedUsername)
    {
        // Usernames are stored normalized (lowercase, trimmed) so this is an exact-match lookup.
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Username == normalizedUsername);
    }
}
