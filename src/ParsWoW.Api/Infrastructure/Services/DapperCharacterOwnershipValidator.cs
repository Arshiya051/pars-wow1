using Dapper;
using ParsWoW.Api.Application.Abstractions.Persistence;
using ParsWoW.Api.Application.Abstractions.Services;
using ParsWoW.Api.Application.Common;
using ParsWoW.Api.Application.Constants;
using ParsWoW.Api.Infrastructure.Persistence;

namespace ParsWoW.Api.Infrastructure.Services;

public sealed class DapperCharacterOwnershipValidator : ICharacterOwnershipValidator
{
    private readonly IExpansionConnectionFactory _conn;

    public DapperCharacterOwnershipValidator(IExpansionConnectionFactory conn) => _conn = conn;

    public async Task<Result<bool>> ValidateCharacterAsync(Guid characterGuid, int accountId, CancellationToken ct = default)
    {
        var expansion = AccountExpansionLocator.Resolve(accountId);
        await using var c = await _conn.OpenAsync(ExpansionDatabase.Characters, expansion, ct);
        const string sql = "SELECT account FROM characters WHERE guid = @Guid LIMIT 1";
        var owner = await c.QuerySingleOrDefaultAsync<uint?>(new CommandDefinition(sql, new { Guid = characterGuid }, cancellationToken: ct));
        if (owner is null) return Result.Fail<bool>("CHARACTER_NOT_FOUND", "Character does not exist.");
        return owner.Value == (uint)accountId
            ? Result.Ok(true)
            : Result.Fail<bool>("NOT_OWNER", "Account does not own this character.");
    }

    public async Task<Result<bool>> ValidateGuildLeaderAsync(int guildId, int accountId, CancellationToken ct = default)
    {
        var expansion = AccountExpansionLocator.Resolve(accountId);
        await using var c = await _conn.OpenAsync(ExpansionDatabase.Characters, expansion, ct);
        const string sql = @"SELECT g.leaderguid, c.account FROM guild g
                              JOIN characters c ON c.guid = g.leaderguid
                              WHERE g.guildid = @G LIMIT 1";
        var row = await c.QuerySingleOrDefaultAsync<(uint leaderGuid, uint account)?>(
            new CommandDefinition(sql, new { G = guildId }, cancellationToken: ct));

        if (row is null) return Result.Fail<bool>("GUILD_NOT_FOUND", "Guild not found.");
        return row.Value.account == (uint)accountId
            ? Result.Ok(true)
            : Result.Fail<bool>("NOT_LEADER", "Account is not the guild leader.");
    }
}

internal static class AccountExpansionLocator
{
    /// <summary>
    /// Resolves which expansion owns the account. Real implementation
    /// queries <c>account.expansion</c>; here we default to WOTLK and
    /// rely on the auth header for explicit expansion routing.
    /// </summary>
    public static ExpansionKind Resolve(int accountId) => ExpansionKind.WOTLK;
}
