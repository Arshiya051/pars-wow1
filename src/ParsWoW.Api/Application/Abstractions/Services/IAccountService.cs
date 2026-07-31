using ParsWoW.Api.Application.Common;
using ParsWoW.Api.Application.Dto.Account;

namespace ParsWoW.Api.Application.Abstractions.Services;

public interface IAccountService
{
    Task<Result<AccountOperationResultDto>> RenameCharacterAsync(CharacterRenameRequest req, CancellationToken ct = default);
    Task<Result<AccountOperationResultDto>> RaceChangeAsync(RaceChangeRequest req, CancellationToken ct = default);
    Task<Result<AccountOperationResultDto>> FactionChangeAsync(FactionChangeRequest req, CancellationToken ct = default);
    Task<Result<AccountOperationResultDto>> AppearanceChangeAsync(AppearanceChangeRequest req, CancellationToken ct = default);
    Task<Result<AccountOperationResultDto>> UnstuckAsync(CharacterUnstuckRequest req, CancellationToken ct = default);
    Task<Result<AccountOperationResultDto>> BoostAsync(CharacterBoostRequest req, CancellationToken ct = default);
    Task<Result<AccountOperationResultDto>> RenameGuildAsync(GuildRenameRequest req, CancellationToken ct = default);
}
