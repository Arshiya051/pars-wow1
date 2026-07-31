using ParsWoW.Api.Application.Common;
using ParsWoW.Api.Application.Constants;

namespace ParsWoW.Api.Application.Abstractions.Services;

/// <summary>
/// Validates that the requesting account owns the targeted character
/// (or leads the targeted guild). Every <c>IAccountService</c>
/// operation MUST pass through this before issuing DML.
/// </summary>
public interface ICharacterOwnershipValidator
{
    Task<Result<bool>> ValidateCharacterAsync(Guid characterGuid, int accountId, CancellationToken ct = default);
    Task<Result<bool>> ValidateGuildLeaderAsync(int guildId, int accountId, CancellationToken ct = default);
}
