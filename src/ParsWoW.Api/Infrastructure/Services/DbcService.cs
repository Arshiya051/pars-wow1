using ParsWoW.Api.Application.Abstractions.Dbc;
using ParsWoW.Api.Application.Abstractions.Services;
using ParsWoW.Api.Application.Common;
using ParsWoW.Api.Application.Dto.Dbc;

namespace ParsWoW.Api.Infrastructure.Services;

/// <summary>
/// Dispatcher for the DBC subsystem. URL slug → IDbcProvider →
/// strongly-typed lookup; the only place that knows about both
/// <c>IDbcProviderFactory</c> and the public DbcDto shapes.
/// </summary>
public sealed class DbcService : IDbcService
{
    private readonly IDbcProviderFactory _factory;

    public DbcService(IDbcProviderFactory factory) => _factory = factory;

    // ============================================================
    // Existing 5 accessors  (kept verbatim)
    // ============================================================

    public async Task<Result<ItemDto>> GetItemAsync(string expansion, int entry, CancellationToken ct = default)
    {
        if (!_factory.TryResolve(expansion, out var provider))
            return Result.Fail<ItemDto>("UNKNOWN_EXPANSION", $"Unknown expansion '{expansion}'.");
        if (!provider.IsLoaded)
            return Result.Fail<ItemDto>("DBC_NOT_LOADED", $"DBC files for '{expansion}' are not loaded yet.");

        var item = provider.GetItem(entry);
        return item is null
            ? Result.Fail<ItemDto>("ITEM_NOT_FOUND", $"Item {entry} not found.")
            : Result.Ok<ItemDto>(new ItemDto
            {
                Entry = item.Entry,
                ClassId = item.ClassId, SubclassId = item.SubclassId,
                SoundOverrideSubclass = item.SoundOverrideSubclass, Material = item.Material,
                DisplayId = item.DisplayId, InventoryType = item.InventoryType, SheatheType = item.SheatheType
            });
    }

    public Task<Result<SpellDto>> GetSpellAsync(string expansion, int spellId, CancellationToken ct = default)
    {
        if (!_factory.TryResolve(expansion, out var provider))
            return Task.FromResult(Result.Fail<SpellDto>("UNKNOWN_EXPANSION", $"Unknown expansion '{expansion}'."));
        var s = provider.GetSpell(spellId);
        return Task.FromResult(s is null
            ? Result.Fail<SpellDto>("SPELL_NOT_FOUND", $"Spell {spellId} not found.")
            : Result.Ok<SpellDto>(new SpellDto
            {
                Id = s.Id, Category = s.Category, Dispel = s.Dispel,
                Mechanic = s.Mechanic, Attributes = s.Attributes, AttributesEx = s.AttributesEx,
                SchoolMask = s.SchoolMask
            }));
    }

    public Task<Result<MapDto>> GetMapAsync(string expansion, int mapId, CancellationToken ct = default)
    {
        if (!_factory.TryResolve(expansion, out var p))
            return Task.FromResult(Result.Fail<MapDto>("UNKNOWN_EXPANSION", $"Unknown expansion '{expansion}'."));
        var m = p.GetMap(mapId);
        return Task.FromResult(m is null
            ? Result.Fail<MapDto>("MAP_NOT_FOUND", $"Map {mapId} not found.")
            : Result.Ok<MapDto>(new MapDto
            { Id = m.Id, InstanceType = m.InstanceType, Flags = m.Flags,
              Directory = m.Directory, MapName = m.MapName }));
    }

    public Task<Result<AreaDto>> GetAreaAsync(string expansion, int areaId, CancellationToken ct = default)
    {
        if (!_factory.TryResolve(expansion, out var p))
            return Task.FromResult(Result.Fail<AreaDto>("UNKNOWN_EXPANSION", $"Unknown expansion '{expansion}'."));
        var a = p.GetArea(areaId);
        return Task.FromResult(a is null
            ? Result.Fail<AreaDto>("AREA_NOT_FOUND", $"Area {areaId} not found.")
            : Result.Ok<AreaDto>(new AreaDto
            { Id = a.Id, ContinentId = a.ContinentId, ParentAreaId = a.ParentAreaId,
              Flags = a.Flags, AreaName = a.AreaName }));
    }

    public Task<Result<AchievementDto>> GetAchievementAsync(string expansion, int achievementId, CancellationToken ct = default)
    {
        if (!_factory.TryResolve(expansion, out var p))
            return Task.FromResult(Result.Fail<AchievementDto>("UNKNOWN_EXPANSION", $"Unknown expansion '{expansion}'."));
        var a = p.GetAchievement(achievementId);
        return Task.FromResult(a is null
            ? Result.Fail<AchievementDto>("ACHIEVEMENT_NOT_FOUND", $"Achievement {achievementId} not found.")
            : Result.Ok<AchievementDto>(new AchievementDto
            { Id = a.Id, Faction = a.Faction, InstanceId = a.InstanceId,
              Category = a.Category, Points = a.Points, OrderInCategory = a.OrderInCategory,
              Title = a.Title, Description = a.Description }));
    }

    // ============================================================
    // 9 new DBC accessors
    // ============================================================

    public Task<Result<FactionDto>> GetFactionAsync(string expansion, int factionId, CancellationToken ct = default)
    {
        if (!_factory.TryResolve(expansion, out var p))
            return Task.FromResult(Result.Fail<FactionDto>("UNKNOWN_EXPANSION", $"Unknown expansion '{expansion}'."));
        var r = p.GetFaction(factionId);
        return Task.FromResult(r is null
            ? Result.Fail<FactionDto>("FACTION_NOT_FOUND", $"Faction {factionId} not found.")
            : Result.Ok<FactionDto>(new FactionDto
            { Id = r.Id, ReputationRaceMask = r.ReputationRaceMask, Name = r.Name,
              Description = r.Description, Flags = r.Flags }));
    }

    public Task<Result<ItemSetDto>> GetItemSetAsync(string expansion, int setId, CancellationToken ct = default)
    {
        if (!_factory.TryResolve(expansion, out var p))
            return Task.FromResult(Result.Fail<ItemSetDto>("UNKNOWN_EXPANSION", $"Unknown expansion '{expansion}'."));
        var r = p.GetItemSet(setId);
        return Task.FromResult(r is null
            ? Result.Fail<ItemSetDto>("ITEMSET_NOT_FOUND", $"ItemSet {setId} not found.")
            : Result.Ok<ItemSetDto>(new ItemSetDto
            { Id = r.Id, Name = r.Name, ItemIds = r.ItemIds, SpellId = r.SpellId }));
    }

    public Task<Result<ItemEnchantmentDto>> GetItemEnchantmentAsync(string expansion, int enchantId, CancellationToken ct = default)
    {
        if (!_factory.TryResolve(expansion, out var p))
            return Task.FromResult(Result.Fail<ItemEnchantmentDto>("UNKNOWN_EXPANSION", $"Unknown expansion '{expansion}'."));
        var r = p.GetItemEnchantment(enchantId);
        return Task.FromResult(r is null
            ? Result.Fail<ItemEnchantmentDto>("ENCHANTMENT_NOT_FOUND", $"Enchantment {enchantId} not found.")
            : Result.Ok<ItemEnchantmentDto>(new ItemEnchantmentDto
            { Id = r.Id, Charges = r.Charges, EffectType = r.EffectType,
              EffectSpellId = r.EffectSpellId, EffectAmount = r.EffectAmount, Name = r.Name }));
    }

    public Task<Result<ChrClassDto>> GetChrClassAsync(string expansion, int classId, CancellationToken ct = default)
    {
        if (!_factory.TryResolve(expansion, out var p))
            return Task.FromResult(Result.Fail<ChrClassDto>("UNKNOWN_EXPANSION", $"Unknown expansion '{expansion}'."));
        var r = p.GetChrClass(classId);
        return Task.FromResult(r is null
            ? Result.Fail<ChrClassDto>("CLASS_NOT_FOUND", $"Class {classId} not found.")
            : Result.Ok<ChrClassDto>(new ChrClassDto { Id = r.Id, Name = r.Name, PowerType = r.PowerType }));
    }

    public Task<Result<ChrRaceDto>> GetChrRaceAsync(string expansion, int raceId, CancellationToken ct = default)
    {
        if (!_factory.TryResolve(expansion, out var p))
            return Task.FromResult(Result.Fail<ChrRaceDto>("UNKNOWN_EXPANSION", $"Unknown expansion '{expansion}'."));
        var r = p.GetChrRace(raceId);
        return Task.FromResult(r is null
            ? Result.Fail<ChrRaceDto>("RACE_NOT_FOUND", $"Race {raceId} not found.")
            : Result.Ok<ChrRaceDto>(new ChrRaceDto { Id = r.Id, Name = r.Name, FactionId = r.FactionId, Flags = r.Flags }));
    }

    public Task<Result<TalentDto>> GetTalentAsync(string expansion, int talentId, CancellationToken ct = default)
    {
        if (!_factory.TryResolve(expansion, out var p))
            return Task.FromResult(Result.Fail<TalentDto>("UNKNOWN_EXPANSION", $"Unknown expansion '{expansion}'."));
        var r = p.GetTalent(talentId);
        return Task.FromResult(r is null
            ? Result.Fail<TalentDto>("TALENT_NOT_FOUND", $"Talent {talentId} not found.")
            : Result.Ok<TalentDto>(new TalentDto
            { Id = r.Id, TabId = r.TabId, TierId = r.TierId,
              ColumnIndex = r.ColumnIndex, SpellId = r.SpellId, Ranks = r.Ranks }));
    }

    public Task<Result<CreatureDisplayDto>> GetCreatureDisplayAsync(string expansion, int displayId, CancellationToken ct = default)
    {
        if (!_factory.TryResolve(expansion, out var p))
            return Task.FromResult(Result.Fail<CreatureDisplayDto>("UNKNOWN_EXPANSION", $"Unknown expansion '{expansion}'."));
        var r = p.GetCreatureDisplay(displayId);
        return Task.FromResult(r is null
            ? Result.Fail<CreatureDisplayDto>("CREATURE_DISPLAY_NOT_FOUND", $"CreatureDisplay {displayId} not found.")
            : Result.Ok<CreatureDisplayDto>(new CreatureDisplayDto
            { Id = r.Id, ModelId = r.ModelId, Texture1 = r.Texture1, Texture2 = r.Texture2, Scale = r.Scale }));
    }

    public Task<Result<ItemDisplayDto>> GetItemDisplayAsync(string expansion, int displayId, CancellationToken ct = default)
    {
        if (!_factory.TryResolve(expansion, out var p))
            return Task.FromResult(Result.Fail<ItemDisplayDto>("UNKNOWN_EXPANSION", $"Unknown expansion '{expansion}'."));
        var r = p.GetItemDisplay(displayId);
        return Task.FromResult(r is null
            ? Result.Fail<ItemDisplayDto>("ITEM_DISPLAY_NOT_FOUND", $"ItemDisplay {displayId} not found.")
            : Result.Ok<ItemDisplayDto>(new ItemDisplayDto
            { Id = r.Id, Model1 = r.Model1, Model2 = r.Model2, Texture = r.Texture, GeosetGroup = r.GeosetGroup }));
    }

    public Task<Result<GemPropertyDto>> GetGemPropertyAsync(string expansion, int gemId, CancellationToken ct = default)
    {
        if (!_factory.TryResolve(expansion, out var p))
            return Task.FromResult(Result.Fail<GemPropertyDto>("UNKNOWN_EXPANSION", $"Unknown expansion '{expansion}'."));
        var r = p.GetGemProperty(gemId);
        return Task.FromResult(r is null
            ? Result.Fail<GemPropertyDto>("GEM_NOT_FOUND", $"GemProperty {gemId} not found.")
            : Result.Ok<GemPropertyDto>(new GemPropertyDto
            { Id = r.Id, SpellItemEnchantment = r.SpellItemEnchantment,
              MaxCount = r.MaxCount, MinLevel = r.MinLevel }));
    }
}
