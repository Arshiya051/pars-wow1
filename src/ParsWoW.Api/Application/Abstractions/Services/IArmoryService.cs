using ParsWoW.Api.Application.Common;
using ParsWoW.Api.Application.Constants;
using ParsWoW.Api.Application.Dto.Armory;

namespace ParsWoW.Api.Application.Abstractions.Services;

public interface IArmoryService
{
    Task<Result<CharacterSummaryDto>> GetCharacterSummaryAsync(ExpansionKind expansion, string realm, string name, CancellationToken ct = default);
    Task<Result<IReadOnlyList<EquipmentItemDto>>> GetEquipmentAsync(ExpansionKind expansion, string realm, string name, CancellationToken ct = default);
    Task<Result<IReadOnlyList<TalentDto>>> GetTalentsAsync(ExpansionKind expansion, string realm, string name, CancellationToken ct = default);
    Task<Result<GuildSummaryDto>> GetGuildSummaryAsync(ExpansionKind expansion, string realm, string name, CancellationToken ct = default);
    Task<Result<CharacterRenderDto>> GetRenderAsync(ExpansionKind expansion, string realm, string name, CancellationToken ct = default);
}
