using Dapper;
using ParsWoW.Api.Application.Abstractions.Auth;
using ParsWoW.Api.Application.Constants;
using ParsWoW.Api.Infrastructure.Persistence;

namespace ParsWoW.Api.Infrastructure.Auth;

public sealed class DapperRefreshTokenStore : IRefreshTokenStore
{
    private readonly IExpansionConnectionFactory _conn;

    public DapperRefreshTokenStore(IExpansionConnectionFactory conn) => _conn = conn;

    public async Task<RefreshTokenRecord> IssueAsync(int accountId, string jti, DateTimeOffset expiresAt, CancellationToken ct = default)
    {
        await using var c = await _conn.OpenAsync(ExpansionDatabase.Auth, ExpansionKind.WOTLK, ct);
        const string sql = @"
            INSERT INTO refresh_tokens (jti, account_id, issued_at, expires_at, revoked, replaced_by)
            VALUES (@Jti, @AccountId, UTC_TIMESTAMP(), @ExpiresAt, 0, NULL)";
        await c.ExecuteAsync(new CommandDefinition(sql, new { Jti = jti, AccountId = accountId, ExpiresAt = expiresAt.UtcDateTime }, cancellationToken: ct));
        return new RefreshTokenRecord
        {
            Jti = jti, AccountId = accountId, IssuedAt = DateTimeOffset.UtcNow, ExpiresAt = expiresAt, IsRevoked = false
        };
    }

    public async Task<RefreshTokenRecord?> GetAsync(string jti, CancellationToken ct = default)
    {
        await using var c = await _conn.OpenAsync(ExpansionDatabase.Auth, ExpansionKind.WOTLK, ct);
        const string sql = @"SELECT jti AS Jti, account_id AS AccountId, issued_at AS IssuedAt,
                                    expires_at AS ExpiresAt, revoked AS IsRevoked, replaced_by AS ReplacedByJti
                               FROM refresh_tokens WHERE jti = @Jti";
        return await c.QuerySingleOrDefaultAsync<RefreshTokenRecord>(new CommandDefinition(sql, new { Jti = jti }, cancellationToken: ct));
    }

    public async Task RevokeAsync(string jti, CancellationToken ct = default)
    {
        await using var c = await _conn.OpenAsync(ExpansionDatabase.Auth, ExpansionKind.WOTLK, ct);
        const string sql = @"UPDATE refresh_tokens SET revoked = 1 WHERE jti = @Jti";
        await c.ExecuteAsync(new CommandDefinition(sql, new { Jti = jti }, cancellationToken: ct));
    }

    public async Task<bool> IsActiveAsync(string jti, CancellationToken ct = default)
    {
        var r = await GetAsync(jti, ct);
        return r is not null && !r.IsRevoked && r.ExpiresAt > DateTimeOffset.UtcNow;
    }
}
