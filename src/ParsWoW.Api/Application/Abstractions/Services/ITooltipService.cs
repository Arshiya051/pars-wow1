using ParsWoW.Api.Application.Common;
using ParsWoW.Api.Application.Constants;
using ParsWoW.Api.Application.Dto.Armory;

namespace ParsWoW.Api.Application.Abstractions.Services;

public interface ITooltipService
{
    /// <summary>
    /// Builds the full tooltip model the Launcher / Website render
    /// directly. Combines the <c>ItemRecord</c> projection, quality
    /// color, sockets, enchant placeholder, and the per-spell trigger
    /// line list.
    /// </summary>
    Task<Result<TooltipDto>> BuildAsync(string expansionSlug, int itemEntry, int? enchantId = null, CancellationToken ct = default);
}
