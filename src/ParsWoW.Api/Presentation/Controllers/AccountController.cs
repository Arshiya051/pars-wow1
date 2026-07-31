using Microsoft.AspNetCore.Mvc;
using ParsWoW.Api.Application.Abstractions.Services;
using ParsWoW.Api.Application.Common;
using ParsWoW.Api.Application.Dto.Account;

namespace ParsWoW.Api.Presentation.Controllers;

/// <summary>Controller for account-level character operations (rename, race-change, faction-change, appearance, unstuck, boost, guild-rename).</summary>
[ApiController]
[Route("api/account")]
public sealed class AccountController : ControllerBase
{
    private readonly IAccountService _accounts;
    /// <summary>Initialises a new <see cref="AccountController"/>.</summary>
    /// <param name="accounts">Account service.</param>
    public AccountController(IAccountService accounts) => _accounts = accounts;

    /// <summary>Queue a character rename. The new name takes effect on next login.</summary>
    [HttpPost("rename")]
    [ProducesResponseType(typeof(ApiResponse<AccountOperationResultDto>), 200)]
    public async Task<IActionResult> Rename([FromBody] CharacterRenameRequest req, CancellationToken ct) =>
        ToResult(await _accounts.RenameCharacterAsync(req, ct));

    /// <summary>Queue a race change for the given character. Takes effect on next login.</summary>
    [HttpPost("race-change")]
    [ProducesResponseType(typeof(ApiResponse<AccountOperationResultDto>), 200)]
    public async Task<IActionResult> RaceChange([FromBody] RaceChangeRequest req, CancellationToken ct) =>
        ToResult(await _accounts.RaceChangeAsync(req, ct));

    /// <summary>Queue a faction change (Alliance ↔ Horde). Takes effect on next login.</summary>
    [HttpPost("faction-change")]
    [ProducesResponseType(typeof(ApiResponse<AccountOperationResultDto>), 200)]
    public async Task<IActionResult> FactionChange([FromBody] FactionChangeRequest req, CancellationToken ct) =>
        ToResult(await _accounts.FactionChangeAsync(req, ct));

    /// <summary>Queue a character appearance change (gender, skin, hair, face). Takes effect on next login.</summary>
    [HttpPost("appearance-change")]
    [ProducesResponseType(typeof(ApiResponse<AccountOperationResultDto>), 200)]
    public async Task<IActionResult> Appearance([FromBody] AppearanceChangeRequest req, CancellationToken ct) =>
        ToResult(await _accounts.AppearanceChangeAsync(req, ct));

    /// <summary>Teleport the character to a safe capital city if stuck.</summary>
    [HttpPost("unstuck")]
    [ProducesResponseType(typeof(ApiResponse<AccountOperationResultDto>), 200)]
    public async Task<IActionResult> Unstuck([FromBody] CharacterUnstuckRequest req, CancellationToken ct) =>
        ToResult(await _accounts.UnstuckAsync(req, ct));

    /// <summary>Boost the character to a target level.</summary>
    [HttpPost("boost")]
    [ProducesResponseType(typeof(ApiResponse<AccountOperationResultDto>), 200)]
    public async Task<IActionResult> Boost([FromBody] CharacterBoostRequest req, CancellationToken ct) =>
        ToResult(await _accounts.BoostAsync(req, ct));

    /// <summary>Rename a guild the account leads.</summary>
    [HttpPost("guild-rename")]
    [ProducesResponseType(typeof(ApiResponse<AccountOperationResultDto>), 200)]
    public async Task<IActionResult> GuildRename([FromBody] GuildRenameRequest req, CancellationToken ct) =>
        ToResult(await _accounts.RenameGuildAsync(req, ct));

    private IActionResult ToResult(Result<AccountOperationResultDto> r) =>
        r.IsSuccess
            ? Ok(ApiResponse<AccountOperationResultDto>.Ok(r.Value!, 200))
            : StatusCode(400, ApiResponse<AccountOperationResultDto>.Fail(400, r.Code ?? "ERROR", r.Error ?? "Operation failed."));
}
