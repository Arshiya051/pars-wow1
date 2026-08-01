using System.Data;
using Dapper;
using ParsWoW.Api.Application.Abstractions.Dbc;
using ParsWoW.Api.Application.Abstractions.Persistence;
using ParsWoW.Api.Application.Abstractions.Services;
using ParsWoW.Api.Application.Common;
using ParsWoW.Api.Application.Constants;
using ParsWoW.Api.Application.Dto.Armory;
using ParsWoW.Api.Infrastructure.Persistence;

namespace ParsWoW.Api.Infrastructure.Services;

public sealed class ArmoryService : IArmoryService
{
    private readonly IExpansionConnectionFactory _conn;
    private readonly IDbcProviderFactory _dbc;
    private readonly ITooltipService _tooltips;

    public ArmoryService(IExpansionConnectionFactory conn, IDbcProviderFactory dbc, ITooltipService tooltips)
    {
        _conn = conn; _dbc = dbc; _tooltips = tooltips;
    }

    // Core-style detection: mangos-based (OregonCore, mangoswotlk) use
    // different column names than Trinity-based (WoWSource, EternityCore, LegionCoreV2).
    private static bool IsMangosCore(ExpansionKind exp) => exp is ExpansionKind.TBC or ExpansionKind.WOTLK;

    // TBC (OregonCore) and WOTLK (mangoswotlk): character_inventory.item_template
    // CATA (WoWSource), MOP (EternityCore), LEGION (LegionCoreV2): character_inventory.item
    private static string InvItemColumn(ExpansionKind exp) => IsMangosCore(exp) ? "item_template" : "item";

    private static string TalentIdColumn(ExpansionKind exp) => IsMangosCore(exp) ? "spell" : "talent_id";

    public async Task<Result<CharacterSummaryDto>> GetCharacterSummaryAsync(
        ExpansionKind expansion, string realm, string name, CancellationToken ct = default)
    {
        await using var c = await _conn.OpenAsync(ExpansionDatabase.Characters, expansion, ct);
        const string sql = @"
            SELECT c.name, c.level, c.race, c.class, c.gender, c.money, c.online,
                   g.name AS guildName
              FROM characters c
              LEFT JOIN guild g ON c.guildid = g.guildid
             WHERE c.name = @Name
             LIMIT 1";
        var row = await c.QuerySingleOrDefaultAsync<(string Name, int Level, int Race, int Class, int Gender, long Money, byte Online, string? GuildName)?>(
            new CommandDefinition(sql, new { Name = name }, cancellationToken: ct));
        if (row is null) return Result.Fail<CharacterSummaryDto>("CHARACTER_NOT_FOUND", $"Character {name} not found on {realm}.");

        return Result.Ok(new CharacterSummaryDto
        {
            Name = row.Value.Name,
            Realm = realm,
            Level = row.Value.Level,
            RaceId = row.Value.Race,
            ClassId = row.Value.Class,
            Gender = row.Value.Gender,
            TotalKills = 0,
            GuildName = row.Value.GuildName ?? string.Empty,
            AchievementPoints = 0,
            LastLogin = DateTimeOffset.UtcNow,
            MoneyCopper = row.Value.Money,
            Faction = ToFaction(row.Value.Race)
        });
    }

    public async Task<Result<IReadOnlyList<EquipmentItemDto>>> GetEquipmentAsync(
        ExpansionKind expansion, string realm, string name, CancellationToken ct = default)
    {
        await using var c = await _conn.OpenAsync(ExpansionDatabase.Characters, expansion, ct);
        var invCol = InvItemColumn(expansion);
        var sql = $@"
            SELECT ci.{invCol} AS ItemEntry, ci.slot
              FROM character_inventory ci
             WHERE ci.guid = (SELECT guid FROM characters WHERE name = @Name LIMIT 1)
               AND ci.bag = 0";
        var rows = await c.QueryAsync<(uint ItemEntry, byte Slot)>(
            new CommandDefinition(sql, new { Name = name }, cancellationToken: ct));
        var equipment = new List<EquipmentItemDto>();

        foreach (var r in rows)
        {
            var item = _dbc.GetProvider(expansion).GetItem((int)r.ItemEntry);
            var tt = await _tooltips.BuildAsync(expansion.ToUrlSlug(), (int)r.ItemEntry, 0, ct);
            equipment.Add(new EquipmentItemDto
            {
                Slot = r.Slot,
                Entry = (int)r.ItemEntry,
                DisplayInfoId = item?.DisplayId ?? 0,
                Quality = 0,
                ItemLevel = 0,
                InventoryType = item?.InventoryType ?? 0,
                ItemClass = item?.ClassId ?? 0,
                Subclass = item?.SubclassId ?? 0,
                RequiredLevel = 0,
                EnchantId = 0,
                RandomProperty = 0,
                RandomSuffix = 0,
                ItemSet = 0,
                Durability = 0,
                MaxDurability = 0,
                SellPrice = 0,
                Tooltip = tt.IsSuccess ? tt.Value! : new TooltipDto()
            });
        }
        return Result.Ok<IReadOnlyList<EquipmentItemDto>>(equipment);
    }

    public async Task<Result<IReadOnlyList<TalentDto>>> GetTalentsAsync(
        ExpansionKind expansion, string realm, string name, CancellationToken ct = default)
    {
        await using var c = await _conn.OpenAsync(ExpansionDatabase.Characters, expansion, ct);
        var talentCol = TalentIdColumn(expansion);
        var sql = $@"
            SELECT ct.{talentCol} AS TalentId, ct.rank
              FROM character_talent ct
             WHERE ct.guid = (SELECT guid FROM characters WHERE name = @Name LIMIT 1)";
        var rows = await c.QueryAsync<(int TalentId, int Rank)>(
            new CommandDefinition(sql, new { Name = name }, cancellationToken: ct));
        var list = rows.Select(r => new TalentDto
        {
            TabId = r.TalentId / 1000,
            Tier = (r.TalentId % 100) / 10,
            Column = r.TalentId % 10,
            SpellId = r.TalentId,
            Rank = r.Rank
        }).ToList();
        return Result.Ok<IReadOnlyList<TalentDto>>(list);
    }

    public async Task<Result<GuildSummaryDto>> GetGuildSummaryAsync(
        ExpansionKind expansion, string realm, string name, CancellationToken ct = default)
    {
        await using var c = await _conn.OpenAsync(ExpansionDatabase.Characters, expansion, ct);
        // All cores: guild table has name, motd, createdate but no 'level' column
        const string sql = @"
            SELECT g.name, g.motd, g.createdate,
                   (SELECT COUNT(*) FROM guild_member gm WHERE gm.guildid = g.guildid) AS memberCount
              FROM guild g
             WHERE g.name = @Name
             LIMIT 1";
        var row = await c.QuerySingleOrDefaultAsync<(string Name, string Motd, long Createdate, int MemberCount)?>(
            new CommandDefinition(sql, new { Name = name }, cancellationToken: ct));
        if (row is null) return Result.Fail<GuildSummaryDto>("GUILD_NOT_FOUND", $"Guild {name} not found.");

        return Result.Ok(new GuildSummaryDto
        {
            Name = row.Value.Name,
            Realm = realm,
            Level = 0,
            MemberCount = row.Value.MemberCount,
            Motd = row.Value.Motd ?? string.Empty,
            AchievementPoints = 0,
            CreatedAt = DateTimeOffset.FromUnixTimeSeconds(row.Value.Createdate)
        });
    }

    public async Task<Result<CharacterRenderDto>> GetRenderAsync(
        ExpansionKind expansion, string realm, string name, CancellationToken ct = default)
    {
        await using var c = await _conn.OpenAsync(ExpansionDatabase.Characters, expansion, ct);
        // mangos: facial_hair column; Trinity: facialStyle column
        var facialCol = IsMangosCore(expansion) ? "facial_hair" : "facialStyle";
        var sql = $@"
            SELECT race, class, gender, face, hair_style, hair_color, skin, {facialCol} AS facial
              FROM characters
             WHERE name = @Name
             LIMIT 1";
        var row = await c.QuerySingleOrDefaultAsync<(byte Race, byte Class, byte Gender, int Face, int HairStyle, int HairColor, int Skin, int Facial)?>(
            new CommandDefinition(sql, new { Name = name }, cancellationToken: ct));
        if (row is null) return Result.Fail<CharacterRenderDto>("CHARACTER_NOT_FOUND", $"Character {name} not found.");

        return Result.Ok(new CharacterRenderDto
        {
            RaceId = row.Value.Race,
            ClassId = row.Value.Class,
            Gender = row.Value.Gender,
            Face = row.Value.Face,
            HairStyle = row.Value.HairStyle,
            HairColor = row.Value.HairColor,
            SkinColor = row.Value.Skin,
            FacialFeatures = row.Value.Facial,
            EquipmentDisplayIds = Array.Empty<int>()
        });
    }

    private static int ToFaction(int race) => race switch
    {
        1 or 3 or 4 or 7 or 11 => 1, // Alliance
        2 or 5 or 6 or 8 or 10 or 9 or 22 or 25 or 26 or 27 or 28 or 29 or 30 or 31 or 32 or 34 or 35 or 36 => 2, // Horde
        _ => 0 // neutral / pandaren etc.
    };
}
