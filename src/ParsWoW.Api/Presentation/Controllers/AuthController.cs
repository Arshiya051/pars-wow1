using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ParsWoW.Api.Application.Abstractions.Services;
using ParsWoW.Api.Application.Common;
using ParsWoW.Api.Application.Dto.Auth;

namespace ParsWoW.Api.Presentation.Controllers;

/// <summary>Controller for authentication: login, register, refresh, logout, current user.</summary>
[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _auth;
    /// <summary>Initialises a new <see cref="AuthController"/>.</summary>
    /// <param name="auth">Auth service.</param>
    public AuthController(IAuthService auth) => _auth = auth;

    /// <summary>Authenticate with username + password. Returns access + refresh tokens.</summary>
    /// <param name="request">Login credentials and target expansion.</param>
    /// <param name="ct">Cancellation token.</param>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<AuthResult>), 200)]
    [ProducesResponseType(typeof(ApiResponse<AuthResult>), 401)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var r = await _auth.LoginAsync(request, ip, ct);
        return ToActionResult(r, 200, 401);
    }

    /// <summary>Register a new account. Returns access + refresh tokens on success.</summary>
    /// <param name="request">New account details.</param>
    /// <param name="ct">Cancellation token.</param>
    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<AuthResult>), 201)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var r = await _auth.RegisterAsync(request, ip, ct);
        return ToActionResult(r, 201, 400);
    }

    /// <summary>Exchange a valid refresh token for a new access + refresh token pair.</summary>
    /// <param name="req">The refresh token from a previous login or refresh.</param>
    /// <param name="ct">Cancellation token.</param>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(ApiResponse<AuthResult>), 200)]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest req, CancellationToken ct)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var r = await _auth.RefreshAsync(req.RefreshToken, ip, ct);
        return ToActionResult(r, 200, 401);
    }

    /// <summary>Revoke a refresh token. Future refresh attempts with it will fail.</summary>
    /// <param name="req">The refresh token to revoke.</param>
    /// <param name="ct">Cancellation token.</param>
    [HttpPost("logout")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    public async Task<IActionResult> Logout([FromBody] RefreshRequest req, CancellationToken ct)
    {
        var r = await _auth.LogoutAsync(req.RefreshToken, ct);
        return ToActionResult(r, 200, 400);
    }

    /// <summary>Returns the current account profile. Requires a valid JWT bearer token.</summary>
    /// <param name="ct">Cancellation token.</param>
    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(ApiResponse<AccountMeDto>), 200)]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        var sub = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                  ?? User.FindFirst("sub")?.Value;
        if (!int.TryParse(sub, out var accountId))
            return Unauthorized(ApiResponse<AccountMeDto>.Fail(401, "INVALID_TOKEN", "Access token is missing subject claim."));
        var r = await _auth.GetCurrentAsync(accountId, ct);
        return ToActionResult(r, 200, 404);
    }

    private IActionResult ToActionResult<T>(Result<T> r, int okStatus, int failStatus) =>
        r.IsSuccess
            ? StatusCode(okStatus, ApiResponse<T>.Ok(r.Value!, okStatus))
            : StatusCode(failStatus, ApiResponse<T>.Fail(failStatus, r.Code ?? "ERROR", r.Error ?? "Operation failed."));
}
