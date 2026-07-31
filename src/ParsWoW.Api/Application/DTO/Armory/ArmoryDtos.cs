namespace ParsWoW.Api.Application.Dto.Armory;

public sealed class CharacterSummaryDto
{
    public required string Name { get; init; }
    public required string Realm { get; init; }
    public int Level { get; init; }
    public int RaceId { get; init; }
    public int ClassId { get; init; }
    public int Gender { get; init; }
    public int TotalKills { get; init; }
    public string GuildName { get; init; } = string.Empty;
    public int AchievementPoints { get; init; }
    public DateTimeOffset? LastLogin { get; init; }
    public long MoneyCopper { get; init; }
    public int Faction { get; init; }
}

public sealed class EquipmentItemDto
{
    public required int Slot { get; init; }
    public required int Entry { get; init; }
    public int DisplayInfoId { get; init; }
    public int Quality { get; init; }
    public int ItemLevel { get; init; }
    public int InventoryType { get; init; }
    public int ItemClass { get; init; }
    public int Subclass { get; init; }
    public int RequiredLevel { get; init; }
    public int RequiredClass { get; init; }
    public int RequiredRace { get; init; }
    public int SocketCount { get; init; }
    public int EnchantId { get; init; }
    public int RandomProperty { get; init; }
    public int RandomSuffix { get; init; }
    public int ItemSet { get; init; }
    public int Durability { get; init; }
    public int MaxDurability { get; init; }
    public long SellPrice { get; init; }
    public TooltipDto Tooltip { get; init; } = new();
}

public sealed class TooltipDto
{
    public int QualityColor { get; init; }
    public string Icon { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string ItemLinkName { get; init; } = string.Empty;
    public int RequiredLevel { get; init; }
    public string RequiredClass { get; init; } = string.Empty;
    public string RequiredRace { get; init; } = string.Empty;
    public int SellPriceCopper { get; init; }
    public IReadOnlyList<string> Stats { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> Sockets { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> SetBonuses { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> Spells { get; init; } = Array.Empty<string>();
    public string Enchant { get; init; } = string.Empty;
    public string FlavorText { get; init; } = string.Empty;
}

public sealed class TalentDto
{
    public required int TabId { get; init; }
    public required int Tier { get; init; }
    public required int Column { get; init; }
    public required int SpellId { get; init; }
    public int Rank { get; init; }
}

public sealed class GuildSummaryDto
{
    public required string Name { get; init; }
    public required string Realm { get; init; }
    public int Level { get; init; }
    public int MemberCount { get; init; }
    public int AchievementPoints { get; init; }
    public string Motd { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
}

public sealed class CharacterRenderDto
{
    public required int RaceId { get; init; }
    public required int ClassId { get; init; }
    public required int Gender { get; init; }
    public int Face { get; init; }
    public int HairStyle { get; init; }
    public int HairColor { get; init; }
    public int SkinColor { get; init; }
    public int FacialFeatures { get; init; }
    public IReadOnlyList<int> EquipmentDisplayIds { get; init; } = Array.Empty<int>();
}
