using Microsoft.AspNetCore.Mvc;
using ParsWoW.Api.Application.Abstractions.Services;
using ParsWoW.Api.Application.Common;
using ParsWoW.Api.Application.Constants;
using ParsWoW.Api.Application.Dto.Dbc;

namespace ParsWoW.Api.Presentation.Controllers;

/// <summary>Controller for expansion-specific DBC file lookups (Item, Spell, Map, Area, Achievement, Faction, etc.).</summary>
[ApiController]
[Route("api/dbc/{expansion}")]
public sealed class DbcController : ControllerBase
{
    private readonly IDbcService _dbc;
    /// <summary>Initialises a new <see cref="DbcController"/>.</summary>
    /// <param name="dbc">DBC service dispatcher.</param>
    public DbcController(IDbcService dbc) => _dbc = dbc;

    // ============================================================
    // Existing 5 routes
    // ============================================================

    /// <summary>Returns a single Item by entry for <paramref name="expansion"/>.</summary>
    /// <remarks>Valid slugs: tbc, wotlk, cata, mop, legion.</remarks>
    [HttpGet("item/{entry:int}")]
    [ProducesResponseType(typeof(ApiResponse<ItemDto>), 200)]
    public async Task<IActionResult> GetItem(string expansion, int entry, CancellationToken ct) =>
        Map(await _dbc.GetItemAsync(expansion, entry, ct));

    /// <summary>Returns a single Spell by identifier for the given expansion.</summary>
    [HttpGet("spell/{spellId:int}")]
    [ProducesResponseType(typeof(ApiResponse<SpellDto>), 200)]
    public async Task<IActionResult> GetSpell(string expansion, int spellId, CancellationToken ct) =>
        Map(await _dbc.GetSpellAsync(expansion, spellId, ct));

    /// <summary>Returns a single Map (instance / continent) by identifier for the given expansion.</summary>
    [HttpGet("map/{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<MapDto>), 200)]
    public async Task<IActionResult> GetMap(string expansion, int id, CancellationToken ct) =>
        Map(await _dbc.GetMapAsync(expansion, id, ct));

    /// <summary>Returns a single Area (zone / subzone) by identifier for the given expansion.</summary>
    [HttpGet("area/{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<AreaDto>), 200)]
    public async Task<IActionResult> GetArea(string expansion, int id, CancellationToken ct) =>
        Map(await _dbc.GetAreaAsync(expansion, id, ct));

    /// <summary>Returns a single Achievement by identifier for the given expansion.</summary>
    [HttpGet("achievement/{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<AchievementDto>), 200)]
    public async Task<IActionResult> GetAchievement(string expansion, int id, CancellationToken ct) =>
        Map(await _dbc.GetAchievementAsync(expansion, id, ct));

    // ============================================================
    // 9 new DBC routes
    // ============================================================

    /// <summary>Returns a single Faction by identifier for the given expansion.</summary>
    [HttpGet("faction/{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<FactionDto>), 200)]
    public async Task<IActionResult> GetFaction(string expansion, int id, CancellationToken ct) =>
        Map(await _dbc.GetFactionAsync(expansion, id, ct));

    /// <summary>Returns a single Item Set (ItemSet.dbc) by identifier for the given expansion.</summary>
    [HttpGet("itemset/{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<ItemSetDto>), 200)]
    public async Task<IActionResult> GetItemSet(string expansion, int id, CancellationToken ct) =>
        Map(await _dbc.GetItemSetAsync(expansion, id, ct));

    /// <summary>Returns a single Item Enchantment (SpellItemEnchantment.dbc) by identifier for the given expansion.</summary>
    [HttpGet("enchantment/{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<ItemEnchantmentDto>), 200)]
    public async Task<IActionResult> GetEnchantment(string expansion, int id, CancellationToken ct) =>
        Map(await _dbc.GetItemEnchantmentAsync(expansion, id, ct));

    /// <summary>Returns a single Character Class (ChrClasses.dbc) by identifier for the given expansion.</summary>
    [HttpGet("class/{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<ChrClassDto>), 200)]
    public async Task<IActionResult> GetClass(string expansion, int id, CancellationToken ct) =>
        Map(await _dbc.GetChrClassAsync(expansion, id, ct));

    /// <summary>Returns a single Character Race (ChrRaces.dbc) by identifier for the given expansion.</summary>
    [HttpGet("race/{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<ChrRaceDto>), 200)]
    public async Task<IActionResult> GetRace(string expansion, int id, CancellationToken ct) =>
        Map(await _dbc.GetChrRaceAsync(expansion, id, ct));

    /// <summary>Returns a single Talent (Talent.dbc) by identifier for the given expansion.</summary>
    [HttpGet("talent/{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<TalentDto>), 200)]
    public async Task<IActionResult> GetTalent(string expansion, int id, CancellationToken ct) =>
        Map(await _dbc.GetTalentAsync(expansion, id, ct));

    /// <summary>Returns a single Creature Display (CreatureDisplayInfo.dbc) by identifier for the given expansion.</summary>
    [HttpGet("creature/{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<CreatureDisplayDto>), 200)]
    public async Task<IActionResult> GetCreature(string expansion, int id, CancellationToken ct) =>
        Map(await _dbc.GetCreatureDisplayAsync(expansion, id, ct));

    /// <summary>Returns a single Item Display (ItemDisplayInfo.dbc) by identifier for the given expansion.</summary>
    [HttpGet("itemdisplay/{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<ItemDisplayDto>), 200)]
    public async Task<IActionResult> GetItemDisplay(string expansion, int id, CancellationToken ct) =>
        Map(await _dbc.GetItemDisplayAsync(expansion, id, ct));

    /// <summary>Returns a single Gem Property (GemProperties.dbc) by identifier for the given expansion.</summary>
    [HttpGet("gem/{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<GemPropertyDto>), 200)]
    public async Task<IActionResult> GetGem(string expansion, int id, CancellationToken ct) =>
        Map(await _dbc.GetGemPropertyAsync(expansion, id, ct));

    private IActionResult Map<T>(Result<T> r) where T : class =>
        r.IsSuccess
            ? Ok(ApiResponse<T>.Ok(r.Value!, 200))
            : StatusCode(r.Code switch
            {
                "UNKNOWN_EXPANSION" => 404,
                "DBC_NOT_LOADED" => 503,
                _ => 404
            }, ApiResponse<T>.Fail(404, r.Code!, r.Error ?? "Not found."));
}
