using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ParsWoW.Api.Application.Abstractions.Dbc;
using ParsWoW.Api.Application.Configuration;
using ParsWoW.Api.Application.Constants;

namespace ParsWoW.Api.Infrastructure.Dbc;

using ParsWoW.Api.Application.Abstractions.Dbc.Records;
using ParsWoW.Api.Infrastructure.Dbc.Engine;

/// <summary>
/// Generic, expansion-agnostic DBC loader base. Subclasses (one per
/// expansion) declare WHICH schemas they ship by passing
/// <c>IEnumerable&lt;IDbcSchema&gt;</c> to this base; everything else
/// is derived from that schema set. Type disambiguation is fully
/// delegated to the schema — <see cref="DbcRecord"/> stores raw uints.
/// </summary>
/// <typeparam name="TSelf">Concrete subclass, for strong-typed registration.</typeparam>
public abstract class DbcProviderBase<TSelf> : IDbcProvider
    where TSelf : DbcProviderBase<TSelf>, IDbcProvider
{
    private readonly IReadOnlyDictionary<string, IDbcSchema> _schemas;
    private readonly IOptions<ParsWowOptions> _options;
    private readonly ILogger<TSelf> _logger;

    protected DbcProviderBase(
        IEnumerable<IDbcSchema> schemas,
        IOptions<ParsWowOptions> options,
        ILogger<TSelf> logger)
    {
        ArgumentNullException.ThrowIfNull(schemas);
        ArgumentNullException.ThrowIfNull(options);
        _logger = logger;

        _schemas = schemas
            .GroupBy(s => s.FileName, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        _options = options;
        RequiredFiles = _schemas.Keys.ToArray();
    }

    public abstract ExpansionKind Expansion { get; }
    public bool IsLoaded { get; private set; }
    public IReadOnlyCollection<string> RequiredFiles { get; }

    // ---------- typed stores ----------
    protected Dictionary<int, ItemRecord> Items { get; } = new();
    protected Dictionary<int, SpellRecord> Spells { get; } = new();
    protected Dictionary<int, MapRecord> Maps { get; } = new();
    protected Dictionary<int, AreaRecord> Areas { get; } = new();
    protected Dictionary<int, AchievementRecord> Achievements { get; } = new();
    protected Dictionary<int, FactionRecord> Factions { get; } = new();
    protected Dictionary<int, ItemSetRecord> ItemSets { get; } = new();
    protected Dictionary<int, ItemEnchantmentRecord> ItemEnchantments { get; } = new();
    protected Dictionary<int, ChrClassRecord> ChrClasses { get; } = new();
    protected Dictionary<int, ChrRaceRecord> ChrRaces { get; } = new();
    protected Dictionary<int, TalentRecord> Talents { get; } = new();
    protected Dictionary<int, CreatureDisplayInfoRecord> CreatureDisplays { get; } = new();
    protected Dictionary<int, ItemDisplayInfoRecord> ItemDisplays { get; } = new();
    protected Dictionary<int, GemPropertiesRecord> GemProperties { get; } = new();

    // ---------- interface accessors ----------
    public IReadOnlyDictionary<int, ItemRecord> AllItems => Items;
    public IReadOnlyDictionary<int, SpellRecord> AllSpells => Spells;
    IReadOnlyDictionary<int, FactionRecord> IDbcProvider.AllFactions => Factions;
    IReadOnlyDictionary<int, ChrClassRecord> IDbcProvider.AllChrClasses => ChrClasses;
    IReadOnlyDictionary<int, ChrRaceRecord> IDbcProvider.AllChrRaces => ChrRaces;
    IReadOnlyDictionary<int, TalentRecord> IDbcProvider.AllTalents => Talents;

    public async Task<IReadOnlyList<string>> LoadAsync(CancellationToken cancellationToken = default)
    {
        if (IsLoaded) return Array.Empty<string>();

        var missing = new List<string>();
        var root = Path.Combine(_options.Value.Dbc.RootPath, Expansion.ToFolderName());

        if (!Directory.Exists(root))
        {
            _logger.LogError("DBC directory {Root} for {Expansion} does not exist.", root, Expansion);
            return RequiredFiles.ToArray();
        }

        foreach (var file in RequiredFiles)
        {
            var path = Path.Combine(root, file);
            if (!File.Exists(path))
            {
                _logger.LogError("Missing DBC file: {File} in {Root} ({Expansion})", file, root, Expansion);
                missing.Add(file);
                continue;
            }

            try
            {
                await using var stream = File.OpenRead(path);
                var dbc = new WdbcReader().Read(stream);

                if (_schemas.TryGetValue(file, out var schema))
                {
                    foreach (var raw in dbc.Records)
                        Accumulate(schema.Project(raw));
                }
                else
                {
                    _logger.LogWarning("DBC file {File} loaded but no schema registered.", file);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load DBC file {File} for {Expansion}", file, Expansion);
                throw;
            }
        }

        IsLoaded = missing.Count == 0;
        return missing;
    }

    /// <summary>
    /// Push a just-projected record into the correct strongly-typed store.
    /// Subclasses can override to seed extension tables.
    /// </summary>
    protected virtual void Accumulate(object record)
    {
        switch (record)
        {
            case ItemRecord item:                  Items[item.Entry] = item; break;
            case SpellRecord spell:                Spells[spell.Id] = spell; break;
            case MapRecord map:                    Maps[map.Id] = map; break;
            case AreaRecord area:                  Areas[area.Id] = area; break;
            case AchievementRecord ach:            Achievements[ach.Id] = ach; break;
            case FactionRecord fac:                Factions[fac.Id] = fac; break;
            case ItemSetRecord set:                ItemSets[set.Id] = set; break;
            case ItemEnchantmentRecord ench:       ItemEnchantments[ench.Id] = ench; break;
            case ChrClassRecord cls:               ChrClasses[cls.Id] = cls; break;
            case ChrRaceRecord race:               ChrRaces[race.Id] = race; break;
            case TalentRecord tal:                 Talents[tal.Id] = tal; break;
            case CreatureDisplayInfoRecord cr:     CreatureDisplays[cr.Id] = cr; break;
            case ItemDisplayInfoRecord disp:       ItemDisplays[disp.Id] = disp; break;
            case GemPropertiesRecord gem:          GemProperties[gem.Id] = gem; break;
        }
    }

    // ---------- Lookup accessors ----------
    public ItemRecord? GetItem(int entry) => Items.TryGetValue(entry, out var v) ? v : null;
    public SpellRecord? GetSpell(int id) => Spells.TryGetValue(id, out var v) ? v : null;
    public MapRecord? GetMap(int id) => Maps.TryGetValue(id, out var v) ? v : null;
    public AreaRecord? GetArea(int id) => Areas.TryGetValue(id, out var v) ? v : null;
    public AchievementRecord? GetAchievement(int id) => Achievements.TryGetValue(id, out var v) ? v : null;
    public FactionRecord? GetFaction(int id) => Factions.TryGetValue(id, out var v) ? v : null;
    public ItemSetRecord? GetItemSet(int id) => ItemSets.TryGetValue(id, out var v) ? v : null;
    public ItemEnchantmentRecord? GetItemEnchantment(int id) => ItemEnchantments.TryGetValue(id, out var v) ? v : null;
    public ChrClassRecord? GetChrClass(int id) => ChrClasses.TryGetValue(id, out var v) ? v : null;
    public ChrRaceRecord? GetChrRace(int id) => ChrRaces.TryGetValue(id, out var v) ? v : null;
    public TalentRecord? GetTalent(int id) => Talents.TryGetValue(id, out var v) ? v : null;
    public CreatureDisplayInfoRecord? GetCreatureDisplay(int id) => CreatureDisplays.TryGetValue(id, out var v) ? v : null;
    public ItemDisplayInfoRecord? GetItemDisplay(int id) => ItemDisplays.TryGetValue(id, out var v) ? v : null;
    public GemPropertiesRecord? GetGemProperty(int id) => GemProperties.TryGetValue(id, out var v) ? v : null;
}
