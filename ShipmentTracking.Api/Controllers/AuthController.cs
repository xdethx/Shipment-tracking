using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShipmentTracking.Core.DTOs;
using ShipmentTracking.Core.Interfaces;

namespace ShipmentTracking.Api.Controllers;

[ApiController]
[Route("api/auth")]
[AllowAnonymous] // login endpoint is public — no token needed to call it
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    // POST /api/auth/login
    // Returns 200 + { token, expiresAt } on success.
    // Returns 401 with a generic message on failure (same message for "unknown user"
    // and "wrong password" — callers cannot probe which usernames exist).
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var response = await _authService.LoginAsync(request);
        if (response is null)
            return Unauthorized(new { message = "Invalid username or password." });

        return Ok(response);
    }
}
