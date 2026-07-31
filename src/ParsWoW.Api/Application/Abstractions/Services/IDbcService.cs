using ParsWoW.Api.Application.Common;
using ParsWoW.Api.Application.Constants;
using ParsWoW.Api.Application.Dto.Dbc;

namespace ParsWoW.Api.Application.Abstractions.Services;

public interface IDbcService
{
    Task<Result<ItemDto>> GetItemAsync(string expansion, int entry, CancellationToken ct = default);
    Task<Result<SpellDto>> GetSpellAsync(string expansion, int spellId, CancellationToken ct = default);
    Task<Result<MapDto>> GetMapAsync(string expansion, int mapId, CancellationToken ct = default);
    Task<Result<AreaDto>> GetAreaAsync(string expansion, int areaId, CancellationToken ct = default);
    Task<Result<AchievementDto>> GetAchievementAsync(string expansion, int achievementId, CancellationToken ct = default);

    // ---------- 9 new ----------
    Task<Result<FactionDto>> GetFactionAsync(string expansion, int factionId, CancellationToken ct = default);
    Task<Result<ItemSetDto>> GetItemSetAsync(string expansion, int setId, CancellationToken ct = default);
    Task<Result<ItemEnchantmentDto>> GetItemEnchantmentAsync(string expansion, int enchantId, CancellationToken ct = default);
    Task<Result<ChrClassDto>> GetChrClassAsync(string expansion, int classId, CancellationToken ct = default);
    Task<Result<ChrRaceDto>> GetChrRaceAsync(string expansion, int raceId, CancellationToken ct = default);
    Task<Result<TalentDto>> GetTalentAsync(string expansion, int talentId, CancellationToken ct = default);
    Task<Result<CreatureDisplayDto>> GetCreatureDisplayAsync(string expansion, int displayId, CancellationToken ct = default);
    Task<Result<ItemDisplayDto>> GetItemDisplayAsync(string expansion, int displayId, CancellationToken ct = default);
    Task<Result<GemPropertyDto>> GetGemPropertyAsync(string expansion, int gemId, CancellationToken ct = default);
}
