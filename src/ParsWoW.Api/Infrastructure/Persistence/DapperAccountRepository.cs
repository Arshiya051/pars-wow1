using Dapper;
using ParsWoW.Api.Application.Abstractions.Persistence;
using ParsWoW.Api.Application.Constants;
using ParsWoW.Api.Infrastructure.Auth;

namespace ParsWoW.Api.Infrastructure.Persistence;

public sealed class DapperAccountRepository : IAccountRepository
{
    private readonly IExpansionConnectionFactory _conn;

    public DapperAccountRepository(IExpansionConnectionFactory conn) => _conn = conn;

    public async Task<AccountRecord?> FindByUsernameAsync(string username, CancellationToken ct = default)
    {
        // All expansions share the same auth database
        await using var c = await _conn.OpenAsync(ExpansionDatabase.Auth, ExpansionKind.WOTLK, ct);
        const string sql = @"
            SELECT id AS Id,
                   username AS Username,
                   email AS Email,
                   sha_pass_hash AS ShaPassHash,
                   v AS VerifierHex,
                   s AS SaltHex,
                   battlenet_account AS BattlenetAccountId,
                   joindate AS Joindate,
                   last_ip AS LastIp,
                   expansion AS Expansion
              FROM account
             WHERE username = @Username
             LIMIT 1";
        return await c.QuerySingleOrDefaultAsync<AccountRecord>(new CommandDefinition(sql,
            new { Username = username }, cancellationToken: ct));
    }

    public async Task<int> CreateAsync(AccountRecord account, CancellationToken ct = default)
    {
        // All expansions share the same auth database
        await using var c = await _conn.OpenAsync(ExpansionDatabase.Auth, ExpansionKind.WOTLK, ct);
        const string sql = @"
            INSERT INTO account (username, sha_pass_hash, v, s, email, joindate, last_ip, battlenet_account, expansion)
            VALUES (@Username, @ShaPassHash, @VerifierHex, @SaltHex, @Email, UTC_TIMESTAMP(), @LastIp, @BattlenetAccountId, @Expansion);
            SELECT LAST_INSERT_ID();";
        return await c.ExecuteScalarAsync<int>(new CommandDefinition(sql, account, cancellationToken: ct));
    }

    public async Task UpdateLastLoginAsync(int accountId, string ip, CancellationToken ct = default)
    {
        await using var c = await _conn.OpenAsync(ExpansionDatabase.Auth, ExpansionKind.WOTLK, ct);
        const string sql = "UPDATE account SET last_ip = @Ip, last_login = UTC_TIMESTAMP() WHERE id = @AccountId";
        await c.ExecuteAsync(new CommandDefinition(sql, new { Ip = ip, AccountId = accountId }, cancellationToken: ct));
    }

    public async Task<int> CreateBattlenetAccountAsync(string email, string shaPassHash, CancellationToken ct = default)
    {
        await using var c = await _conn.OpenAsync(ExpansionDatabase.Auth, ExpansionKind.WOTLK, ct);
        const string sql = @"
            INSERT INTO battlenet_accounts (email, sha_pass_hash, joindate)
            VALUES (@Email, @ShaPassHash, UTC_TIMESTAMP());
            SELECT LAST_INSERT_ID();";
        return await c.ExecuteScalarAsync<int>(new CommandDefinition(sql,
            new { Email = email, ShaPassHash = shaPassHash },
            cancellationToken: ct));
    }
}