using ParsWoW.Api.Application.Abstractions.Persistence;
using ParsWoW.Api.Application.Abstractions.Services;
using ParsWoW.Api.Application.Common;
using ParsWoW.Api.Application.Constants;
using ParsWoW.Api.Application.Dto.Account;
using ParsWoW.Api.Infrastructure.Persistence;

namespace ParsWoW.Api.Infrastructure.Services;

public sealed class AccountService : IAccountService
{
    private readonly IExpansionConnectionFactory _conn;
    private readonly ICharacterOwnershipValidator _ownership;

    public AccountService(IExpansionConnectionFactory conn, ICharacterOwnershipValidator ownership)
    {
        _conn = conn; _ownership = ownership;
    }

    public async Task<Result<AccountOperationResultDto>> RenameCharacterAsync(CharacterRenameRequest req, CancellationToken ct = default)
    {
        var owns = await _ownership.ValidateCharacterAsync(req.CharacterGuid, req.AccountId, ct);
        if (!owns.IsSuccess) return Result.Fail<AccountOperationResultDto>(owns.Code!, owns.Error!);

        await using var c = await _conn.OpenAsync(ExpansionDatabase.Characters, DetectExpansion(req.CharacterGuid), ct);
        const string sql = @"UPDATE characters SET name = @NewName, at_login_flags = at_login_flags | 1 WHERE guid = @Guid";
        await Dapper.SqlMapper.ExecuteAsync(c, new Dapper.CommandDefinition(sql,
            new { NewName = req.NewName, Guid = req.CharacterGuid }, cancellationToken: ct));

        return Result.Ok(new AccountOperationResultDto
        {
            Success = true, Operation = "rename-character",
            Message = $"Queued rename: {req.NewName}",
            AffectedEntityId = req.CharacterGuid.GetHashCode()
        });
    }

    public async Task<Result<AccountOperationResultDto>> RaceChangeAsync(RaceChangeRequest req, CancellationToken ct = default)
    {
        var owns = await _ownership.ValidateCharacterAsync(req.CharacterGuid, req.AccountId, ct);
        if (!owns.IsSuccess) return Result.Fail<AccountOperationResultDto>(owns.Code!, owns.Error!);

        await using var c = await _conn.OpenAsync(ExpansionDatabase.Characters, DetectExpansion(req.CharacterGuid), ct);
        const string sql = "UPDATE characters SET race = @Race, at_login_flags = at_login_flags | 64 WHERE guid = @Guid";
        await Dapper.SqlMapper.ExecuteAsync(c, new Dapper.CommandDefinition(sql,
            new { Race = req.NewRaceId, Guid = req.CharacterGuid }, cancellationToken: ct));

        return Result.Ok(new AccountOperationResultDto
        {
            Success = true, Operation = "race-change",
            Message = $"Queued race change to {req.NewRaceId}."
        });
    }

    public async Task<Result<AccountOperationResultDto>> FactionChangeAsync(FactionChangeRequest req, CancellationToken ct = default)
    {
        var owns = await _ownership.ValidateCharacterAsync(req.CharacterGuid, req.AccountId, ct);
        if (!owns.IsSuccess) return Result.Fail<AccountOperationResultDto>(owns.Code!, owns.Error!);

        await using var c = await _conn.OpenAsync(ExpansionDatabase.Characters, DetectExpansion(req.CharacterGuid), ct);
        const string sql = "UPDATE characters SET race = (CASE WHEN race IN (1,3,4,7,11) THEN race + 1 ELSE race - 1 END) WHERE guid = @Guid";
        await Dapper.SqlMapper.ExecuteAsync(c, new Dapper.CommandDefinition(sql,
            new { Guid = req.CharacterGuid }, cancellationToken: ct));

        return Result.Ok(new AccountOperationResultDto
        {
            Success = true, Operation = "faction-change",
            Message = "Queued faction change."
        });
    }

    public async Task<Result<AccountOperationResultDto>> AppearanceChangeAsync(AppearanceChangeRequest req, CancellationToken ct = default)
    {
        var owns = await _ownership.ValidateCharacterAsync(req.CharacterGuid, req.AccountId, ct);
        if (!owns.IsSuccess) return Result.Fail<AccountOperationResultDto>(owns.Code!, owns.Error!);

        await using var c = await _conn.OpenAsync(ExpansionDatabase.Characters, DetectExpansion(req.CharacterGuid), ct);
        // mangos uses facial_hair; Trinity uses facialStyle
        var facialCol = "facial_hair";
        var sql = $@"UPDATE characters
                        SET gender = @Gender, skin = @SkinColor, face = @Face,
                            hair_style = @HairStyle, hair_color = @HairColor, {facialCol} = @Facial,
                            at_login_flags = at_login_flags | 8
                      WHERE guid = @Guid";
        await Dapper.SqlMapper.ExecuteAsync(c, new Dapper.CommandDefinition(sql, new
        {
            Gender = req.Gender, SkinColor = req.SkinColor, Face = req.Face,
            HairStyle = req.HairStyleId, HairColor = req.HairColorId, Facial = req.FacialFeatures,
            Guid = req.CharacterGuid
        }, cancellationToken: ct));

        return Result.Ok(new AccountOperationResultDto { Success = true, Operation = "appearance-change" });
    }

    public async Task<Result<AccountOperationResultDto>> UnstuckAsync(CharacterUnstuckRequest req, CancellationToken ct = default)
    {
        var owns = await _ownership.ValidateCharacterAsync(req.CharacterGuid, req.AccountId, ct);
        if (!owns.IsSuccess) return Result.Fail<AccountOperationResultDto>(owns.Code!, owns.Error!);

        await using var c = await _conn.OpenAsync(ExpansionDatabase.Characters, DetectExpansion(req.CharacterGuid), ct);
        const string sql = "UPDATE characters SET position_x = -8949.95, position_y = -132.92, position_z = 83.53, map = 0 WHERE guid = @Guid";
        await Dapper.SqlMapper.ExecuteAsync(c, new Dapper.CommandDefinition(sql,
            new { Guid = req.CharacterGuid }, cancellationToken: ct));

        return Result.Ok(new AccountOperationResultDto { Success = true, Operation = "unstuck", Message = "Teleported to capital." });
    }

    public async Task<Result<AccountOperationResultDto>> BoostAsync(CharacterBoostRequest req, CancellationToken ct = default)
    {
        var owns = await _ownership.ValidateCharacterAsync(req.CharacterGuid, req.AccountId, ct);
        if (!owns.IsSuccess) return Result.Fail<AccountOperationResultDto>(owns.Code!, owns.Error!);

        await using var c = await _conn.OpenAsync(ExpansionDatabase.Characters, DetectExpansion(req.CharacterGuid), ct);
        const string sql = @"UPDATE characters SET level = @Level, xp = 0 WHERE guid = @Guid";
        await Dapper.SqlMapper.ExecuteAsync(c, new Dapper.CommandDefinition(sql,
            new { Level = req.TargetLevel, Guid = req.CharacterGuid }, cancellationToken: ct));

        return Result.Ok(new AccountOperationResultDto { Success = true, Operation = "boost", Message = $"Boosted to {req.TargetLevel}." });
    }

    public async Task<Result<AccountOperationResultDto>> RenameGuildAsync(GuildRenameRequest req, CancellationToken ct = default)
    {
        await using var c = await _conn.OpenAsync(ExpansionDatabase.Characters, ExpansionKind.WOTLK, ct);
        var ownedBy = await Dapper.SqlMapper.QuerySingleOrDefaultAsync<int?>(c,
            new Dapper.CommandDefinition("SELECT leaderguid FROM guild WHERE guildid = @G", new { G = req.GuildId }, cancellationToken: ct));
        if (ownedBy is null) return Result.Fail<AccountOperationResultDto>("GUILD_NOT_FOUND", "Guild not found.");

        var charAccountId = await Dapper.SqlMapper.QuerySingleOrDefaultAsync<int?>(c,
            new Dapper.CommandDefinition("SELECT account FROM characters WHERE guid = @G", new { G = ownedBy.Value }, cancellationToken: ct));
        if (charAccountId != req.AccountId)
            return Result.Fail<AccountOperationResultDto>("NOT_GUILD_LEADER", "You do not lead that guild.");

        await Dapper.SqlMapper.ExecuteAsync(c, new Dapper.CommandDefinition(
            "UPDATE guild SET name = @N WHERE guildid = @G", new { N = req.NewName, G = req.GuildId }, cancellationToken: ct));
        return Result.Ok(new AccountOperationResultDto { Success = true, Operation = "guild-rename", Message = $"Renamed to {req.NewName}." });
    }

    private static ExpansionKind DetectExpansion(Guid characterGuid)
    {
        // By convention, accounts store their active expansion in account.expansion
        // and characters.guids are globally unique. For routing, callers should
        // pass the expansion explicitly; we default to WOTLK for safety.
        return ExpansionKind.WOTLK;
    }

}

