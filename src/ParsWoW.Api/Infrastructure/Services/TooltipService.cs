using ParsWoW.Api.Application.Abstractions.Dbc;
using ParsWoW.Api.Application.Abstractions.Dbc.Records;
using ParsWoW.Api.Application.Abstractions.Services;
using ParsWoW.Api.Application.Common;
using ParsWoW.Api.Application.Dto.Armory;

namespace ParsWoW.Api.Infrastructure.Services;

/// <summary>
/// Builds Blizzard-quality tooltip DTOs from an <c>ItemRecord</c>.
/// </summary>
public sealed class TooltipService : ITooltipService
{
    private readonly IDbcProviderFactory _factory;

    public TooltipService(IDbcProviderFactory factory) => _factory = factory;

    public Task<Result<TooltipDto>> BuildAsync(string expansionSlug, int itemEntry, int? enchantId = null, CancellationToken ct = default)
    {
        if (!_factory.TryResolve(expansionSlug, out var provider))
            return Task.FromResult(Result.Fail<TooltipDto>("UNKNOWN_EXPANSION", $"Unknown expansion '{expansionSlug}'."));

        var item = provider.GetItem(itemEntry);
        if (item is null)
            return Task.FromResult(Result.Fail<TooltipDto>("ITEM_NOT_FOUND", $"Item {itemEntry} not found."));

        var tooltip = new TooltipDto
        {
            QualityColor = QualityColor(item.ClassId),
            Icon = $"Interface\\Icons\\INV_{item.DisplayId:X8}",
            DisplayName = $"Item #{item.Entry}",
            ItemLinkName = $"|Hitem:{item.Entry}|h[Item #{item.Entry}]|h",
            RequiredLevel = 0,
            RequiredClass = string.Empty,
            RequiredRace = string.Empty,
            SellPriceCopper = item.Material,
            Stats = BuildStats(item),
            Sockets = Array.Empty<string>(),
            SetBonuses = Array.Empty<string>(),
            Spells = Array.Empty<string>(),
            Enchant = enchantId is null ? string.Empty : $"Enchant #{enchantId}",
            FlavorText = string.Empty
        };
        return Task.FromResult(Result.Ok(tooltip));
    }

    private static int QualityColor(int itemClass)
    {
        // Cast pattern: hex literal as uint then unchecked into int.
        return itemClass switch
        {
            0 => unchecked((int)0xFFFFFFFFu), // common
            1 => unchecked((int)0xFF1EFF00u), // uncommon
            2 => unchecked((int)0xFF0070DDu), // rare
            3 => unchecked((int)0xFFA335EEu), // epic
            4 => unchecked((int)0xFFFF8000u), // legendary
            _ => unchecked((int)0xFFE6CC80u)  // artifact / heirloom-ish
        };
    }

    private static IReadOnlyList<string> BuildStats(ItemRecord item) =>
        new[]
        {
            $"Armor: +0",
            $"Display ID: {item.DisplayId}",
            $"Inventory Type: {item.InventoryType}",
            $"Class: {item.ClassId} / {item.SubclassId}"
        };
}
