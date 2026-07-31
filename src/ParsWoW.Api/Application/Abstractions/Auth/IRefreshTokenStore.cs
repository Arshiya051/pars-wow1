namespace ParsWoW.Api.Application.Abstractions.Auth;

/// <summary>
/// Refresh-token store abstraction. Returns rotation / revocation state
/// for each issued refresh token; consumed by <c>AuthService</c> and
/// <c>AuthController</c> on every refresh / logout flow.
/// </summary>
public interface IRefreshTokenStore
{
    Task<RefreshTokenRecord> IssueAsync(int accountId, string jti, DateTimeOffset expiresAt, CancellationToken ct = default);
    Task<RefreshTokenRecord?> GetAsync(string jti, CancellationToken ct = default);
    Task RevokeAsync(string jti, CancellationToken ct = default);
    Task<bool> IsActiveAsync(string jti, CancellationToken ct = default);
}

public sealed class RefreshTokenRecord
{
    public required string Jti { get; init; }
    public required int AccountId { get; init; }
    public required DateTimeOffset IssuedAt { get; init; }
    public required DateTimeOffset ExpiresAt { get; init; }
    public required bool IsRevoked { get; set; }
    public string? ReplacedByJti { get; set; }
}
