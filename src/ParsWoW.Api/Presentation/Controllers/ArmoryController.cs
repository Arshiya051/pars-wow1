using Microsoft.AspNetCore.Mvc;
using ParsWoW.Api.Application.Abstractions.Services;
using ParsWoW.Api.Application.Common;
using ParsWoW.Api.Application.Constants;
using ParsWoW.Api.Application.Dto.Armory;

namespace ParsWoW.Api.Presentation.Controllers;

/// <summary>Controller for character armory data: summary, equipment, talents, render, guild.</summary>
[ApiController]
[Route("api/armory/{expansion}")]
public sealed class ArmoryController : ControllerBase
{
    private readonly IArmoryService _armory;
    /// <summary>Initialises a new <see cref="ArmoryController"/>.</summary>
    /// <param name="armory">Armory service.</param>
    public ArmoryController(IArmoryService armory) => _armory = armory;

    /// <summary>Returns the character summary (level, race, class, guild, money) for the given realm + name.</summary>
    /// <param name="expansion">One of: tbc, wotlk, cata, mop, legion.</param>
    /// <param name="realm">Realm / server name.</param>
    /// <param name="name">Character name.</param>
    /// <param name="ct">Cancellation token.</param>
    [HttpGet("character/{realm}/{name}/summary")]
    [ProducesResponseType(typeof(ApiResponse<CharacterSummaryDto>), 200)]
    public async Task<IActionResult> Summary(string expansion, string realm, string name, CancellationToken ct)
    {
        if (!ExpansionKindExtensions.TryParseSlug(expansion, out var kind))
            return Err("UNKNOWN_EXPANSION", $"Unknown expansion '{expansion}'.", 404);
        return OkOrNot(await _armory.GetCharacterSummaryAsync(kind, realm, name, ct));
    }

    /// <summary>Returns the character's equipped items, each with a rendered tooltip.</summary>
    [HttpGet("character/{realm}/{name}/equipment")]
    [ProducesResponseType(typeof(ApiResponse<EquipmentItemDto>), 200)]
    public async Task<IActionResult> Equipment(string expansion, string realm, string name, CancellationToken ct)
    {
        if (!ExpansionKindExtensions.TryParseSlug(expansion, out var kind))
            return Err("UNKNOWN_EXPANSION", $"Unknown expansion '{expansion}'.", 404);
        return OkOrNot(await _armory.GetEquipmentAsync(kind, realm, name, ct));
    }

    /// <summary>Returns the character's talent points, tab, tier, and column layout.</summary>
    [HttpGet("character/{realm}/{name}/talents")]
    [ProducesResponseType(typeof(ApiResponse<TalentDto>), 200)]
    public async Task<IActionResult> Talents(string expansion, string realm, string name, CancellationToken ct)
    {
        if (!ExpansionKindExtensions.TryParseSlug(expansion, out var kind))
            return Err("UNKNOWN_EXPANSION", $"Unknown expansion '{expansion}'.", 404);
        return OkOrNot(await _armory.GetTalentsAsync(kind, realm, name, ct));
    }

    /// <summary>Returns the character's appearance data (face, hair, skin, equipment displays) for rendering purposes.</summary>
    [HttpGet("character/{realm}/{name}/render")]
    [ProducesResponseType(typeof(ApiResponse<CharacterRenderDto>), 200)]
    public async Task<IActionResult> Render(string expansion, string realm, string name, CancellationToken ct)
    {
        if (!ExpansionKindExtensions.TryParseSlug(expansion, out var kind))
            return Err("UNKNOWN_EXPANSION", $"Unknown expansion '{expansion}'.", 404);
        return OkOrNot(await _armory.GetRenderAsync(kind, realm, name, ct));
    }

    /// <summary>Returns guild summary (level, member count, MOTD, created date) for the given realm + name.</summary>
    [HttpGet("guild/{realm}/{name}/summary")]
    [ProducesResponseType(typeof(ApiResponse<GuildSummaryDto>), 200)]
    public async Task<IActionResult> Guild(string expansion, string realm, string name, CancellationToken ct)
    {
        if (!ExpansionKindExtensions.TryParseSlug(expansion, out var kind))
            return Err("UNKNOWN_EXPANSION", $"Unknown expansion '{expansion}'.", 404);
        return OkOrNot(await _armory.GetGuildSummaryAsync(kind, realm, name, ct));
    }

    private IActionResult OkOrNot<T>(Result<T> r) where T : class =>
        r.IsSuccess ? Ok(ApiResponse<T>.Ok(r.Value!, 200))
                    : StatusCode(404, ApiResponse<T>.Fail(404, r.Code!, r.Error ?? "Not found."));

    private IActionResult Err(string code, string message, int status) =>
        StatusCode(status, ApiResponse<object>.Fail(status, code, message));
}
