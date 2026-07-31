using ParsWoW.Api.Application.Constants;

namespace ParsWoW.Api.Application.Abstractions.Dbc;

using ParsWoW.Api.Application.Abstractions.Dbc.Records;

/// <summary>
/// Per-expansion DBC data provider. Each expansion ships its own
/// <c>IDbcProvider</c> implementation that loads ONLY its own schema
/// bundle (see <c>Schemas/{TBC,WOTLK,…}</c>). The generic loader
/// (<c>DbcProviderBase</c>) walks the file system and dispatches rows to
/// the correct schema; this interface exposes the strongly typed lookups.
/// </summary>
public interface IDbcProvider
{
    ExpansionKind Expansion { get; }
    Task<IReadOnlyList<string>> LoadAsync(CancellationToken cancellationToken = default);
    bool IsLoaded { get; }
    IReadOnlyCollection<string> RequiredFiles { get; }

    // ---------- Item ----------
    ItemRecord? GetItem(int entry);
    IReadOnlyDictionary<int, ItemRecord> AllItems { get; }

    // ---------- Spell ----------
    SpellRecord? GetSpell(int id);
    IReadOnlyDictionary<int, SpellRecord> AllSpells { get; }

    // ---------- Map, Area, Achievement ----------
    MapRecord? GetMap(int id);
    AreaRecord? GetArea(int id);
    AchievementRecord? GetAchievement(int id);

    // ---------- Faction ----------
    FactionRecord? GetFaction(int id);

    // ---------- ItemSet ----------
    ItemSetRecord? GetItemSet(int id);

    // ---------- ItemEnchantment (SpellItemEnchantment.dbc) ----------
    ItemEnchantmentRecord? GetItemEnchantment(int id);

    // ---------- ChrClasses ----------
    ChrClassRecord? GetChrClass(int id);

    // ---------- ChrRaces ----------
    ChrRaceRecord? GetChrRace(int id);

    // ---------- Talent ----------
    TalentRecord? GetTalent(int id);

    // ---------- CreatureDisplayInfo ----------
    CreatureDisplayInfoRecord? GetCreatureDisplay(int id);

    // ---------- ItemDisplayInfo ----------
    ItemDisplayInfoRecord? GetItemDisplay(int id);

    // ---------- GemProperties ----------
    GemPropertiesRecord? GetGemProperty(int id);

    // ---------- Bulk lookup (lazy populated by providers at load time) ----------
    IReadOnlyDictionary<int, FactionRecord> AllFactions { get; }
    IReadOnlyDictionary<int, ChrClassRecord> AllChrClasses { get; }
    IReadOnlyDictionary<int, ChrRaceRecord> AllChrRaces { get; }
    IReadOnlyDictionary<int, TalentRecord> AllTalents { get; }
}
