using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ShipmentTracking.Core.Constants;
using ShipmentTracking.Core.Entities;

namespace ShipmentTracking.Infrastructure.Data;

public static class AdminUserSeeder
{
    // Called once at startup. Creates the initial admin account if:
    //   1. The Users table is empty (no existing accounts), AND
    //   2. Seed:AdminUsername and Seed:AdminPassword are both configured.
    //
    // If either seed config value is missing, seeding is silently skipped —
    // there is NO built-in default account. This prevents a well-known password
    // from being active when the app is deployed without seed config.
    public static async Task SeedAsync(
        AppDbContext context,
        IPasswordHasher<AppUser> hasher,
        IConfiguration configuration)
    {
        if (await context.Users.AnyAsync())
            return;

        var username = configuration["Seed:AdminUsername"];
        var password = configuration["Seed:AdminPassword"];

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            return;

        var admin = new AppUser
        {
            Username = username.Trim().ToLowerInvariant(),
            Role     = Roles.Admin,
        };

        // Hash BEFORE persisting — PasswordHash must never hold a plaintext value.
        admin.PasswordHash = hasher.HashPassword(admin, password);

        await context.Users.AddAsync(admin);
        await context.SaveChangesAsync();
    }
}
