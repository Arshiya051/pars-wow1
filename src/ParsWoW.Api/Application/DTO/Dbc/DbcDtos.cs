namespace ParsWoW.Api.Application.Dto.Dbc;

// ---------- existing (kept verbatim) ----------

public sealed class ItemDto
{
    public required int Entry { get; init; }
    public int ClassId { get; init; }
    public int SubclassId { get; init; }
    public int SoundOverrideSubclass { get; init; }
    public int Material { get; init; }
    public int DisplayId { get; init; }
    public int InventoryType { get; init; }
    public int SheatheType { get; init; }
}

public sealed class SpellDto
{
    public required int Id { get; init; }
    public int Category { get; init; }
    public int Dispel { get; init; }
    public int Mechanic { get; init; }
    public int Attributes { get; init; }
    public int AttributesEx { get; init; }
    public int SchoolMask { get; init; }
}

public sealed class MapDto
{
    public required int Id { get; init; }
    public int InstanceType { get; init; }
    public int Flags { get; init; }
    public string Directory { get; init; } = string.Empty;
    public string MapName { get; init; } = string.Empty;
}

public sealed class AreaDto
{
    public required int Id { get; init; }
    public int ContinentId { get; init; }
    public int ParentAreaId { get; init; }
    public int Flags { get; init; }
    public string AreaName { get; init; } = string.Empty;
}

public sealed class AchievementDto
{
    public required int Id { get; init; }
    public int Faction { get; init; }
    public int InstanceId { get; init; }
    public int Category { get; init; }
    public int Points { get; init; }
    public int OrderInCategory { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}

// ---------- 9 new DTO types ----------

public sealed class FactionDto
{
    public required int Id { get; init; }
    public int ReputationRaceMask { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public int Flags { get; init; }
}

public sealed class ItemSetDto
{
    public required int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public IReadOnlyList<int> ItemIds { get; init; } = Array.Empty<int>();
    public int SpellId { get; init; }
}

public sealed class ItemEnchantmentDto
{
    public required int Id { get; init; }
    public int Charges { get; init; }
    public int EffectType { get; init; }
    public int EffectSpellId { get; init; }
    public int EffectAmount { get; init; }
    public string Name { get; init; } = string.Empty;
}

public sealed class ChrClassDto
{
    public required int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public int PowerType { get; init; }
}

public sealed class ChrRaceDto
{
    public required int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public int FactionId { get; init; }
    public int Flags { get; init; }
}

public sealed class TalentDto
{
    public required int Id { get; init; }
    public int TabId { get; init; }
    public int TierId { get; init; }
    public int ColumnIndex { get; init; }
    public int SpellId { get; init; }
    public int Ranks { get; init; }
}

public sealed class CreatureDisplayDto
{
    public required int Id { get; init; }
    public int ModelId { get; init; }
    public string Texture1 { get; init; } = string.Empty;
    public string Texture2 { get; init; } = string.Empty;
    public float Scale { get; init; }
}

public sealed class ItemDisplayDto
{
    public required int Id { get; init; }
    public string Model1 { get; init; } = string.Empty;
    public string Model2 { get; init; } = string.Empty;
    public string Texture { get; init; } = string.Empty;
    public int GeosetGroup { get; init; }
}

public sealed class GemPropertyDto
{
    public required int Id { get; init; }
    public int SpellItemEnchantment { get; init; }
    public int MaxCount { get; init; }
    public int MinLevel { get; init; }
}
