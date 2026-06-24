using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using ShipmentTracking.Core.Auth;
using ShipmentTracking.Core.Constants;
using ShipmentTracking.Core.DTOs;
using ShipmentTracking.Core.Entities;
using ShipmentTracking.Core.Interfaces;

namespace ShipmentTracking.Core.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher<AppUser> _hasher;
    private readonly JwtSettings _jwtSettings;

    public AuthService(
        IUserRepository userRepository,
        IPasswordHasher<AppUser> hasher,
        JwtSettings jwtSettings)
    {
        _userRepository = userRepository;
        _hasher         = hasher;
        _jwtSettings    = jwtSettings;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        // Normalize so "Admin", " admin ", and "ADMIN" all match the stored entry.
        var normalizedUsername = request.Username.Trim().ToLowerInvariant();
        var user = await _userRepository.GetByUsernameAsync(normalizedUsername);

        // Return null for "not found" AND "wrong password" — both map to the same 401.
        // This prevents callers from probing which usernames exist.
        if (user is null)
            return null;

        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
            return null;

        return BuildToken(user);
    }

    private LoginResponse BuildToken(AppUser user)
    {
        var expiresAt   = DateTime.UtcNow.AddHours(_jwtSettings.ExpiryHours);
        var keyBytes    = Encoding.UTF8.GetBytes(_jwtSettings.Key);
        var signingKey  = new SymmetricSecurityKey(keyBytes);
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        // Claims embedded in the token:
        //   sub          — user id (standard JWT subject)
        //   unique_name  — username, readable in the admin UI
        //   role         — used by [Authorize(Roles = "Admin")] on the server
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
            new Claim(ClaimTypes.Role, user.Role),
        };

        var token = new JwtSecurityToken(
            issuer:            _jwtSettings.Issuer,
            audience:          _jwtSettings.Audience,
            claims:            claims,
            expires:           expiresAt,
            signingCredentials: credentials);

        return new LoginResponse
        {
            Token     = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiresAt = expiresAt,
        };
    }
}
